import os

def generate_tree(startpath, root_name):
    # Setup for the root
    print(f"📦 {root_name}")
    
    # Exclusions
    exclude_dirs = {'.git', '.vs', 'bin', 'obj', 'Properties', 'Service References'} 
    # Note: Properties and Service References were INCLUDED in the example, so I should NOT exclude them.
    # Re-reading example: 
    # ┃ ┣ 📂 Properties
    # ┃ ┣ 📂 Service References
    # So I only exclude .git, .vs, bin, obj
    exclude_dirs = {'.git', '.vs', 'bin', 'obj'}
    
    # Helper to traverse
    def walk(path, prefix=""):
        # Get list of items
        try:
            items = os.listdir(path)
        except PermissionError:
            return

        # Filter items
        filtered_items = []
        for item in items:
            if item in exclude_dirs:
                continue
            if item.startswith('.'): # exclude hidden files mostly
                continue
            filtered_items.append(item)
        
        # Sort: Directories first, then files. Both alphabetical.
        filtered_items.sort(key=lambda x: (not os.path.isdir(os.path.join(path, x)), x.lower()))

        # Iterate
        for i, item in enumerate(filtered_items):
            is_last = (i == len(filtered_items) - 1)
            full_path = os.path.join(path, item)
            is_dir = os.path.isdir(full_path)
            
            connector = "┗ " if is_last else "┣ "
            # Unicode spacing is tricky, plain spaces usually work if font is mono
            # But the example uses " ┃ " for continuation. 
            
            icon = "📂" if is_dir else "📜"
            
            print(f"{prefix}{connector}{icon} {item}")
            
            if is_dir:
                extension = "   " if is_last else "┃ "
                walk(full_path, prefix + extension)

    walk(startpath, " ")

if __name__ == "__main__":
    # Current directory is the workspace root
    generate_tree(".", "arcgis_engine_project")
