Import-Module .\SqlServerCacheClient.dll

Write-Host "Testing powershell functions... you must have a local database called Cache for this to work...."

function VerifyCounter($expectedValue) {
	$counter = Get-SqlCacheCounter -SchemaName "cache" -Database "Cache" -Key "testCounter1"
	if ($counter -ne $expectedValue ) { throw "invalid value for counter." }
}

Write-Host "Testing Counter...."
Set-SqlCacheCounter -SchemaName "cache" -Database "Cache" -Key "testCounter1" -Count 111 -verbose
VerifyCounter 111
Set-SqlCacheCounter -SchemaName "cache" -Database "Cache" -Key "testCounter1" -Increment -verbose
VerifyCounter 112
Set-SqlCacheCounter -SchemaName "cache" -Database "Cache" -Key "testCounter1" -Decrement -verbose
VerifyCounter 111
Remove-SqlCacheCounter -SchemaName "cache" -Database "Cache" -Key "testCounter1" -verbose
$counter = Get-SqlCacheCounter -SchemaName "cache" -Database "Cache" -Key "testCounter1"
if ($counter) { throw "failed counter removal. " }
Set-SqlCacheCounter -SchemaName "cache" -Database "Cache" -Key "testCounter1" -Increment -verbose
VerifyCounter 1

Write-Host "Counter test successful."

function VerifyTextCache($expectedValue) {
	$text = Get-SqlCacheText -SchemaName "cache" -Database "Cache" -Key "textkey1"
	if ($text -ne $expectedValue) { throw "invalid value for text cache." }
}
Write-Host "Testing cache text."
Set-SqlCacheText -SchemaName "cache" -Database "Cache" -Key "textkey1" -Text "blah blah blah" -Verbose
VerifyTextCache "blah blah blah"
Set-SqlCacheText -SchemaName "cache" -Database "Cache" -Key "textkey1" -Text "blah blah blah blah blah" -Verbose
VerifyTextCache "blah blah blah blah blah"
Remove-SqlCacheText -SchemaName "cache" -Database "Cache" -Key "textkey1" -Verbose
	$text = Get-SqlCacheText -SchemaName "cache" -Database "Cache" -Key "textkey1"
	if ($text) { throw "invalid value for text cache." }

Write-Host "Text cache test successful."

$binaryObject = @{blah="ya"; hooey = 123; yo = Get-Date}
function VerifyBinaryCache($expectedValue) {
	$object = Get-SqlCacheObject -SchemaName "cache" -Database "Cache" -Key "binKey1"
	if (-not ($object.blah -eq $binaryObject.blah -and $object.hooey -eq $binaryObject.hooey -and $object.yo -eq $binaryObject.yo )) { throw "invalid value for binary cache." }
}

Write-Host "Testing binary cache."
Set-SqlCacheObject -SchemaName "cache" -Database "Cache" -Key "binKey1" -Value $binaryObject
VerifyBinaryCache $binaryObject
$binaryObject.hooey = 124
Set-SqlCacheObject -SchemaName "cache" -Database "Cache" -Key "binKey1" -Value $binaryObject
VerifyBinaryCache $binaryObject
Remove-SqlCacheObject -SchemaName "cache" -Database "Cache" -Key "binKey1"
$object = Get-SqlCacheObject -SchemaName "cache" -Database "Cache" -Key "binKey1"
if ($object) { throw "invalid value for binary cache." }
Write-Host "Binary cache test successful."
