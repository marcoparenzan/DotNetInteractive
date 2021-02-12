Invoke-RestMethod -Uri "https://localhost:5001/notebook?name=sample" -Method Post -InFile "sampleNotebook.ipynb" -UseDefaultCredentials

# Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
