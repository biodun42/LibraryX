# PowerShell script to update Identity files to use ApplicationUser

# Add the Models namespace to all CS files
Get-ChildItem -Path "LibraryX\Areas\Identity" -Filter "*.cs" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    if ($content -notmatch "using LibraryX.Models;") {
        $content = $content -replace "using Microsoft\.Extensions\.Logging;", "using Microsoft.Extensions.Logging;`r`nusing LibraryX.Models;"
        Set-Content -Path $_.FullName -Value $content
    }
}

# Replace IdentityUser with ApplicationUser in all CS files
Get-ChildItem -Path "LibraryX\Areas\Identity" -Filter "*.cs" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $updatedContent = $content -replace "IdentityUser", "ApplicationUser"
    if ($updatedContent -ne $content) {
        Set-Content -Path $_.FullName -Value $updatedContent
    }
}

# Replace IdentityUser with ApplicationUser in all CSHTML files
Get-ChildItem -Path "LibraryX\Areas\Identity" -Filter "*.cshtml" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $updatedContent = $content -replace "IdentityUser", "ApplicationUser"
    if ($updatedContent -ne $content) {
        Set-Content -Path $_.FullName -Value $updatedContent
    }
}

Write-Host "Identity files updated successfully!"
