# Build image
docker build -f IdentityApi\Dockerfile --force-rm -t identity-image .

# Compose container
docker compose -f IdentityApi\docker-compose.yml -p identity up -d --no-deps --build

# Show docker container
docker ps --filter "name=identity-identity-app-1"

# small delay
Start-Sleep -Seconds 1.5
Write-Output "Copying data file into docker container mounted volume"

# Copy data to volume (overwrite)
docker cp 'IdentityApi/var/data/medlemmer.json' 'identity-identity-app-1:/app/var/data/'
