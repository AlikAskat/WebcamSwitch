$id = 'USB\VID_3277&PID_0096&MI_00\7&EAD33BB&0&0000'
$log = 'C:\Projects\WinUnbury\WebcamSwitch\toggle.log'

try {
    Import-Module PnpDevice -ErrorAction Stop

    $dev = Get-PnpDevice -InstanceId $id -ErrorAction Stop
    "BEFORE: Status=$($dev.Status), Class=$($dev.Class), Name=$($dev.FriendlyName)" | Out-File $log -Encoding UTF8

    if ($dev.Status -eq 'OK') {
        Disable-PnpDevice -InstanceId $id -Confirm:$false -ErrorAction Stop
        "ACTION: DISABLE" | Add-Content $log
    }
    else {
        Enable-PnpDevice -InstanceId $id -Confirm:$false -ErrorAction Stop
        "ACTION: ENABLE" | Add-Content $log
    }

    Start-Sleep -Seconds 2

    $after = Get-PnpDevice -InstanceId $id -ErrorAction Stop
    "AFTER: Status=$($after.Status)" | Add-Content $log
}
catch {
    "ERROR: $($_.Exception.Message)" | Add-Content $log
    "FULL: $($_ | Out-String)" | Add-Content $log
}
