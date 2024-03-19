

# StartDecryptionService.ps1

param(
    [string]$Registry = "localhost:5000"
)

. .\ps-scripts\StartContainerRegistry.ps1

kubectl create namespace 'decryption-service'

docker build -t decryption-master:latest ./Workers/Master
docker build -t decryption-client:latest ./Workers/Client

# Tag the images with the registry's address
docker tag decryption-master:latest $Registry/decryption-master:latest
docker tag decryption-client:latest $Registry/decryption-client:latest

# Push the images to the local registry
docker push $Registry/decryption-master:latest
docker push $Registry/decryption-client:latest




# Apply the master deployment
Write-Host "Applying master deployment..."
kubectl apply -f deployments/decryption-master-deployment.yaml

# Apply the client deployment
Write-Host "Applying client deployment..."
kubectl apply -f deployments/decryption-client-deployment.yaml


# Apply the master service
Write-Host "Applying master service..."
kubectl apply -f services/decryption-master-service.yaml
Pop-Location