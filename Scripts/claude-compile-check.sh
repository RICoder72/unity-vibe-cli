#!/bin/bash

# claude-compile-check.sh - Unity compilation validator for claude-code integration
# 
# Purpose: Validates Unity C# script compilation after claude-code makes changes
# Returns structured output with error/warning details and precise file locations
#
# Exit Codes:
#   0 = Success (no compilation errors)
#   1 = Compilation errors found
#   2 = Compilation timeout or Unity not accessible
#   3 = Script execution error
#
# Usage: ./claude-compile-check.sh [--include-warnings]

UNITY_LOG_PATH="/mnt/c/Users/matth/AppData/Local/Unity/Editor/Editor.log"
INCLUDE_WARNINGS=false
SCRIPT_VERSION="1.3.0"

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --include-warnings)
            INCLUDE_WARNINGS=true
            shift
            ;;
        --help|-h)
            echo "Usage: $0 [--include-warnings]"
            echo "Validates Unity compilation for claude-code integration"
            echo ""
            echo "Options:"
            echo "  --include-warnings  Include warning details in output"
            echo "  --help, -h          Show this help message"
            echo ""
            echo "Exit codes:"
            echo "  0 = Success (no errors)"
            echo "  1 = Compilation errors found"
            echo "  2 = Timeout/Unity not found"
            echo "  3 = Script execution error"
            exit 0
            ;;
        *)
            echo "ERROR: Unknown option $1" >&2
            echo "Use --help for usage information" >&2
            exit 3
            ;;
    esac
done

# Function to output structured results
output_result() {
    local status="$1"
    local error_count="$2"
    local warning_count="$3"
    local details="$4"
    
    echo "STATUS: $status"
    echo "ERRORS: $error_count"
    echo "WARNINGS: $warning_count"
    if [[ -n "$details" ]]; then
        echo "DETAILS:"
        echo "$details"
    fi
    echo "SCRIPT_VERSION: $SCRIPT_VERSION"
}

# Function to focus Unity window for recompilation
focus_unity() {
    powershell.exe -Command "
    \$unity = Get-Process -Name 'Unity' -ErrorAction SilentlyContinue | Where-Object { \$_.MainWindowTitle -like '*vibe-unity-testrig*' } | Select-Object -First 1;
    if (\$unity) {
        Add-Type -AssemblyName Microsoft.VisualBasic;
        [Microsoft.VisualBasic.Interaction]::AppActivate(\$unity.Id);
        Write-Host 'Unity focused for compilation check'
    } else {
        Write-Host 'Unity process not found'
        exit 1
    }" 2>/dev/null
    
    return $?
}

# Function to focus back to WSL terminal
focus_wsl() {
    powershell.exe -Command "
    \$wsl = Get-Process -Name 'WindowsTerminal' -ErrorAction SilentlyContinue | Select-Object -First 1;
    if (\$wsl) {
        Add-Type -AssemblyName Microsoft.VisualBasic;
        [Microsoft.VisualBasic.Interaction]::AppActivate(\$wsl.Id);
    } else {
        \$cmd = Get-Process -Name 'cmd' -ErrorAction SilentlyContinue | Select-Object -First 1;
        if (\$cmd) {
            Add-Type -AssemblyName Microsoft.VisualBasic;
            [Microsoft.VisualBasic.Interaction]::AppActivate(\$cmd.Id);
        }
    }" 2>/dev/null
}

# Function to parse compilation errors and warnings from Unity log
parse_compilation_results() {
    local log_content="$1"
    local errors=""
    local warnings=""
    local error_count=0
    local warning_count=0
    
    # Parse compilation errors
    while IFS= read -r line; do
        if [[ "$line" =~ ^(.+)\(([0-9]+),([0-9]+)\):\ error\ (.+):\ (.+)$ ]]; then
            # C# compilation error format: File(line,col): error CS####: Message
            local file="${BASH_REMATCH[1]}"
            local line_num="${BASH_REMATCH[2]}"
            local error_msg="${BASH_REMATCH[5]}"
            
            # Clean up file path for better readability
            file=$(echo "$file" | sed 's|.*[/\\]Packages[/\\]|./Packages/|' | sed 's|.*[/\\]Assets[/\\]|./Assets/|')
            
            errors="${errors}  [$file:$line_num] $error_msg\n"
            ((error_count++))
        elif [[ "$line" =~ error\ CS[0-9]+: ]]; then
            # Generic error pattern
            local error_msg=$(echo "$line" | sed 's/.*error CS[0-9]*: //')
            errors="${errors}  [Unknown] $error_msg\n"
            ((error_count++))
        fi
    done <<< "$log_content"
    
    # Parse warnings if requested
    if [[ "$INCLUDE_WARNINGS" == "true" ]]; then
        while IFS= read -r line; do
            if [[ "$line" =~ ^(.+)\(([0-9]+),([0-9]+)\):\ warning\ (.+):\ (.+)$ ]]; then
                # C# compilation warning format: File(line,col): warning CS####: Message
                local file="${BASH_REMATCH[1]}"
                local line_num="${BASH_REMATCH[2]}"
                local warning_msg="${BASH_REMATCH[5]}"
                
                # Clean up file path
                file=$(echo "$file" | sed 's|.*[/\\]Packages[/\\]|./Packages/|' | sed 's|.*[/\\]Assets[/\\]|./Assets/|')
                
                warnings="${warnings}  [$file:$line_num] WARNING: $warning_msg\n"
                ((warning_count++))
            elif [[ "$line" =~ warning\ CS[0-9]+: ]]; then
                # Generic warning pattern
                local warning_msg=$(echo "$line" | sed 's/.*warning CS[0-9]*: //')
                warnings="${warnings}  [Unknown] WARNING: $warning_msg\n"
                ((warning_count++))
            fi
        done <<< "$log_content"
    fi
    
    # Combine results
    local details=""
    if [[ -n "$errors" ]]; then
        details="${details}${errors}"
    fi
    if [[ -n "$warnings" ]]; then
        details="${details}${warnings}"
    fi
    
    echo "$error_count|$warning_count|$details"
}

# Function to monitor Unity compilation with timeout
monitor_compilation() {
    local timeout=45  # Increased timeout for compilation checking
    local elapsed=0
    
    # Verify Unity log exists
    if [[ ! -f "$UNITY_LOG_PATH" ]]; then
        output_result "ERROR" "0" "0" "Unity Editor log not found at: $UNITY_LOG_PATH"
        return 2
    fi
    
    # Get initial log size to track new entries
    local initial_size=$(stat -c%s "$UNITY_LOG_PATH")
    local compilation_started=false
    
    while [[ $elapsed -lt $timeout ]]; do
        local current_size=$(stat -c%s "$UNITY_LOG_PATH")
        
        if [[ $current_size -gt $initial_size ]]; then
            # Get new log entries since monitoring started
            local new_entries=$(tail -c +$((initial_size + 1)) "$UNITY_LOG_PATH")
            
            # Check for compilation start indicators
            if [[ "$compilation_started" == "false" ]] && echo "$new_entries" | grep -q "Reloading assemblies\|CompileScripts\|Start importing"; then
                compilation_started=true
            fi
            
            # Check for compilation completion indicators
            if echo "$new_entries" | grep -q "Reloading assemblies after forced synchronous recompile\|Finished compiling graph\|CompileScripts:.*ms\|Hotreload:.*ms\|PostProcessAllAssets:.*ms"; then
                
                # Parse the compilation results
                local parse_result=$(parse_compilation_results "$new_entries")
                local error_count=$(echo "$parse_result" | cut -d'|' -f1)
                local warning_count=$(echo "$parse_result" | cut -d'|' -f2)
                local details=$(echo "$parse_result" | cut -d'|' -f3-)
                
                if [[ $error_count -eq 0 ]]; then
                    output_result "SUCCESS" "$error_count" "$warning_count" "$details"
                    return 0
                else
                    output_result "ERRORS" "$error_count" "$warning_count" "$details"
                    return 1
                fi
            fi
            
            # Check for compilation errors in real-time
            if echo "$new_entries" | grep -q "error CS[0-9]*:"; then
                # Found errors, but wait a bit more to get complete error list
                sleep 2
                
                # Get final log state
                local final_entries=$(tail -c +$((initial_size + 1)) "$UNITY_LOG_PATH")
                local parse_result=$(parse_compilation_results "$final_entries")
                local error_count=$(echo "$parse_result" | cut -d'|' -f1)
                local warning_count=$(echo "$parse_result" | cut -d'|' -f2)
                local details=$(echo "$parse_result" | cut -d'|' -f3-)
                
                output_result "ERRORS" "$error_count" "$warning_count" "$details"
                return 1
            fi
        fi
        
        sleep 1
        ((elapsed++))
    done
    
    # Timeout reached
    if [[ "$compilation_started" == "true" ]]; then
        output_result "TIMEOUT" "0" "0" "Compilation started but did not complete within ${timeout}s"
    else
        output_result "TIMEOUT" "0" "0" "No compilation activity detected within ${timeout}s. Unity may not be responding."
    fi
    return 2
}

# Main execution function
main() {
    # Step 1: Focus Unity to trigger compilation
    if ! focus_unity; then
        output_result "ERROR" "0" "0" "Failed to focus Unity window. Ensure Unity is running with vibe-unity-testrig project."
        return 2
    fi
    
    # Brief pause to allow Unity to process the focus
    sleep 2
    
    # Step 2: Focus back to WSL terminal
    focus_wsl
    sleep 1
    
    # Step 3: Monitor compilation and return results
    monitor_compilation
    return $?
}

# Execute main function if script is run directly
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi