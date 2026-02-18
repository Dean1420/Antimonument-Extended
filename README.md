# Antimonument-Extended

## setting up "git-lfs"
´´´
# 1. Install Git LFS (if not already installed)
sudo apt install git-lfs

# 2. Initialize Git LFS in your repository
cd /path/to/your/repo
git lfs install

# 3. Track the file types you want (Unity common files)
git lfs track "*.png"
git lfs track "*.jpg"
git lfs track "*.jpeg"
git lfs track "*.psd"
git lfs track "*.fbx"
git lfs track "*.glb"
git lfs track "*.gltf"
git lfs track "*.wav"
git lfs track "*.mp3"
git lfs track "*.mp4"

# 4. Add and commit the .gitattributes file
git add .gitattributes
git commit -m "Add Git LFS tracking"

# 5. Add your files normally
git add .
git commit -m "Add project files"
git push

# 7 Check tracked patterns
git lfs track

# 7 See which files are in LFS
git lfs ls-files
´´´
