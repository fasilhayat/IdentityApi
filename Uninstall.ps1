# STOP AND REMOVE CONTAINERS
docker stop identity-identity-app-1
docker rm -f identity-identity-app-1

# REMOVE IMAGES
docker rmi identity-image

# REMOVE ALL UNUSED VOLUMES
docker volume rm identity_identity-app