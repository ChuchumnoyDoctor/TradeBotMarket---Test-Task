# Создаем директорию для сертификатов, если она не существует
New-Item -ItemType Directory -Force -Path .\certs

# Генерируем самоподписанный сертификат
$cert = New-SelfSignedCertificate `
    -DnsName "localhost" `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -NotAfter (Get-Date).AddYears(1) `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -KeyExportPolicy Exportable `
    -Subject "CN=localhost"

# Экспортируем сертификат в PFX файл
$password = ConvertTo-SecureString -String "your-password" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath ".\certs\localhost.pfx" -Password $password

# Экспортируем публичный ключ сертификата
Export-Certificate -Cert $cert -FilePath ".\certs\localhost.crt" -Type CERT