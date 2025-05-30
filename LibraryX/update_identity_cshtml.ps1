"Get-ChildItem -Path 'LibraryX\Areas\Identity' -Filter '*.cshtml' -Recurse | ForEach-Object { (Get-Content $_.FullName) -replace 'IdentityUser', 'ApplicationUser' | Set-Content $_.FullName }" 
