# PowerShell script to add LibraryX.Models namespace to all Identity CS files

$files = Get-ChildItem -Path "LibraryX\Areas\Identity" -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "obj|bin" }

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Skip files that already have the namespace
    if ($content -match "using LibraryX\.Models;") {
        Write-Host "Skipping $($file.FullName) - already has LibraryX.Models namespace"
        continue
    }
    
    # Insert namespace after the last using statement
    $newContent = $content -replace "(?<=using [^;]+;)(?!\s*using)", "`r`nusing LibraryX.Models;"
    
    # Write the updated content back to the file
    Set-Content -Path $file.FullName -Value $newContent
    
    Write-Host "Updated $($file.FullName)"
}

Write-Host "Namespace update complete!"
