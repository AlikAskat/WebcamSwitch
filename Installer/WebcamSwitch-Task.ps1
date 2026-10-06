$deviceId = 'USB\VID_3277&PID_0096&MI_00\7&EAD33BB&0&0000'
$pnputil = "$env:SystemRoot\System32\pnputil.exe"

$device = Get-PnpDevice -InstanceId $deviceId -ErrorAction Stop

if ($device.Status -eq 'OK') {
    & $pnputil /disable-device $deviceId /force
}
else {
    & $pnputil /enable-device $deviceId
}

exit $LASTEXITCODE
