# StartDecryptionService.ps1

# Generate a new GUID for the image tag
$imageTag = [guid]::NewGuid().ToString()

# Create namespace if we haven't already and clear up
kubectl create namespace 'decryption-service'
kubectl delete --all deployments -n decryption-service

docker build -t decryption-master:$imageTag ./Workers/Master
docker build -t decryption-client:$imageTag ./Workers/Client

# Apply the master deployment
Write-Host "Applying master deployment..."
kubectl apply -f deployments/decryption-master-deployment.yaml

# Apply the client deployment
Write-Host "Applying client deployment..."
kubectl apply -f deployments/decryption-client-deployment.yaml

# Apply the master service
Write-Host "Applying master service..."
kubectl apply -f services/decryption-master-service.yaml

# A broken container will start up before we set image here
# In prod envs would be using kustomize etc so the templating is done properly
kubectl set image deployment/decryption-master decryption-master=decryption-master:$imageTag -n decryption-service
kubectl set image deployment/decryption-client decryption-client=decryption-client:$imageTag -n decryption-service
