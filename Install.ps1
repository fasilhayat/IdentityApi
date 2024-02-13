# Build image
docker build -f IdentityApi\Dockerfile --force-rm -t identity-image .

# Compose container
docker compose -f IdentityApi\docker-compose.yml -p identity up -d --no-deps --build

# Copy data to volume (overwrite)
docker cp 'IdentityApi/var/data/medlemmer.json' 'identity-identity-app-1:/app/var/data/'
