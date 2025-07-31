#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Net;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace VibeUnity.Editor
{
    /// <summary>
    /// HTTP Server for Vibe Unity - Allows external command execution while Unity is running
    /// This provides a REST API endpoint that the CLI can communicate with directly
    /// </summary>
    [InitializeOnLoad]
    public static class VibeUnityHttpServer
    {
        private static HttpListener listener;
        private static Thread listenerThread;
        private static bool isRunning = false;
        
        public static bool IsRunning => isRunning;
        private const int PORT = 9876;
        
        static VibeUnityHttpServer()
        {
            EditorApplication.update += Initialize;
        }
        
        private static void Initialize()
        {
            EditorApplication.update -= Initialize;
            // Check if enabled in preferences
            if (VibeUnityMenu.IsHttpServerEnabled)
            {
                StartServerInternal();
            }
        }
        
        public static void StartServerInternal()
        {
            if (isRunning)
            {
                Debug.Log("[VibeUnityHTTP] Server already running on port " + PORT);
                return;
            }
            
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add($"http://*:{PORT}/");
                listener.Start();
                isRunning = true;
                
                listenerThread = new Thread(ListenForRequests);
                listenerThread.Start();
                
                Debug.Log($"[VibeUnityHTTP] Server started on http://localhost:{PORT}/");
                Debug.Log("[VibeUnityHTTP] CLI can now send commands directly to Unity");
                
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
                EditorApplication.quitting += StopServerInternal;
            }
            catch (Exception e)
            {
                Debug.LogError($"[VibeUnityHTTP] Failed to start server: {e.Message}");
            }
        }
        
        public static void StopServerInternal()
        {
            if (!isRunning) return;
            
            isRunning = false;
            listener?.Stop();
            listenerThread?.Join(1000);
            
            Debug.Log("[VibeUnityHTTP] Server stopped");
        }
        
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            // Keep server running during play mode
        }
        
        private static void ListenForRequests()
        {
            while (isRunning)
            {
                try
                {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => ProcessRequest(context));
                }
                catch (Exception e)
                {
                    if (isRunning)
                        Debug.LogError($"[UnityVibeHTTP] Listen error: {e.Message}");
                }
            }
        }
        
        private static void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            
            try
            {
                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/execute")
                {
                    using (var reader = new System.IO.StreamReader(request.InputStream))
                    {
                        var json = reader.ReadToEnd();
                        
                        // Manual JSON parsing for simplicity
                        var actionMatch = System.Text.RegularExpressions.Regex.Match(json, @"""action""\s*:\s*""([^""]+)""");
                        var paramsMatch = System.Text.RegularExpressions.Regex.Match(json, @"""parameters""\s*:\s*(\{[^}]+\})");
                        
                        if (!actionMatch.Success)
                        {
                            throw new Exception("Missing action in request");
                        }
                        
                        var command = new CliCommand
                        {
                            action = actionMatch.Groups[1].Value,
                            parametersJson = paramsMatch.Success ? paramsMatch.Groups[1].Value : "{}"
                        };
                        
                        // Execute on main thread
                        bool executed = false;
                        string result = "";
                        bool success = false;
                        
                        EditorApplication.delayCall += () =>
                        {
                            try
                            {
                                result = ExecuteCommand(command);
                                success = !result.StartsWith("Error:");
                                executed = true;
                            }
                            catch (Exception e)
                            {
                                result = $"Error: {e.Message}";
                                success = false;
                                executed = true;
                            }
                        };
                        
                        // Wait for execution (with timeout)
                        int timeout = 5000;
                        while (!executed && timeout > 0)
                        {
                            Thread.Sleep(100);
                            timeout -= 100;
                        }
                        
                        var responseJson = $"{{\"success\":{success.ToString().ToLower()},\"result\":\"{result}\"}}";
                        var responseBytes = Encoding.UTF8.GetBytes(responseJson);
                        
                        response.ContentType = "application/json";
                        response.ContentLength64 = responseBytes.Length;
                        response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                    }
                }
                else
                {
                    response.StatusCode = 404;
                    var responseBytes = Encoding.UTF8.GetBytes("Not Found");
                    response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityVibeHTTP] Process error: {e.Message}");
                response.StatusCode = 500;
            }
            finally
            {
                response.Close();
            }
        }
        
        private static string ExecuteCommand(CliCommand command)
        {
            // Simple manual parsing since Unity's JsonUtility is limited
            switch (command.action)
            {
                case "create-scene":
                    // Parse parameters manually from JSON
                    var createMatch = System.Text.RegularExpressions.Regex.Match(
                        command.parametersJson, 
                        @"""name""\s*:\s*""([^""]+)""\s*,\s*""path""\s*:\s*""([^""]+)""");
                    
                    if (createMatch.Success)
                    {
                        var sceneName = createMatch.Groups[1].Value;
                        var scenePath = createMatch.Groups[2].Value;
                        
                        // Parse optional parameters
                        var typeMatch = System.Text.RegularExpressions.Regex.Match(
                            command.parametersJson, @"""type""\s*:\s*""([^""]+)""");
                        var sceneType = typeMatch.Success ? typeMatch.Groups[1].Value : "DefaultGameObjects";
                        
                        var buildMatch = System.Text.RegularExpressions.Regex.Match(
                            command.parametersJson, @"""addToBuild""\s*:\s*(true|false)");
                        var addToBuild = buildMatch.Success && buildMatch.Groups[1].Value == "true";
                        
                        bool success = CLI.CreateScene(sceneName, scenePath, sceneType, addToBuild);
                        return success ? $"Scene created: {scenePath}/{sceneName}.unity" : "Failed to create scene";
                    }
                    return "Error: Invalid create-scene parameters";
                    
                case "add-canvas":
                    var canvasMatch = System.Text.RegularExpressions.Regex.Match(
                        command.parametersJson, @"""name""\s*:\s*""([^""]+)""");
                    
                    if (canvasMatch.Success)
                    {
                        var canvasName = canvasMatch.Groups[1].Value;
                        
                        var modeMatch = System.Text.RegularExpressions.Regex.Match(
                            command.parametersJson, @"""renderMode""\s*:\s*""([^""]+)""");
                        var renderMode = modeMatch.Success ? modeMatch.Groups[1].Value : "ScreenSpaceOverlay";
                        
                        bool success = CLI.AddCanvas(canvasName, renderMode);
                        return success ? $"Canvas created: {canvasName}" : "Failed to create canvas";
                    }
                    return "Error: Invalid add-canvas parameters";
                    
                default:
                    return $"Unknown command: {command.action}";
            }
        }
        
        [Serializable]
        private class CliCommand
        {
            public string action;
            public string parametersJson; // Keep parameters as raw JSON string
        }
    }
}
#endif