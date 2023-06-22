Write-Host "Pack..."
dotnet pack -c Release
Write-Host "Push..."
dotnet nuget push CommonBuild\bin\Release\MyCompany.CommonBuild.*.nupkg
Read-Host -Prompt "Press Enter to exit"