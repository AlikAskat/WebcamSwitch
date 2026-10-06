$taskName = "WebcamSwitch"
$scriptPath = Join-Path $PSScriptRoot "WebcamSwitch-Task.ps1"

$action = New-ScheduledTaskAction `
    -Execute "powershell.exe" `
    -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`""

$principal = New-ScheduledTaskPrincipal `
    -UserId "SYSTEM" `
    -LogonType ServiceAccount `
    -RunLevel Highest

Register-ScheduledTask `
    -TaskName $taskName `
    -Action $action `
    -Principal $principal `
    -Force

$userSid = [System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value

$svc = New-Object -ComObject "Schedule.Service"
$svc.Connect()

$task = $svc.GetFolder("\").GetTask($taskName)
$sddl = $task.GetSecurityDescriptor(4)

$oldAce = "(A;;FR;;;$userSid)"
$newAce = "(A;;FRFX;;;$userSid)"

if ($sddl.Contains($oldAce)) {
    $sddl = $sddl.Replace($oldAce, $newAce)
}
elseif (-not $sddl.Contains($newAce)) {
    $sddl += $newAce
}

$task.SetSecurityDescriptor($sddl, 0)
