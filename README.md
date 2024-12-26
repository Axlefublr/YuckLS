Highly experimental partially functional LSP implementation for the yuck configuration language. See https://www.github.com/elkowar/eww.
## Building.
Install dotnet-sdk-8.0 on your computer from your package manager.

# clone and compile
```
git clone https://www.github.com/eugenenoble2005/yuckls.git
cd yuckls/YuckLS
dotnet build --output dist
```
Note where you cloned the repo to and where the compiled binaries are (/dist). You can now set it up for an editor
# Install from aur
You can install the master branch from the aur.
```
yay -S yuckls-git
```
The executable can be started by running ```yuckls```

# Neovim
```
-- put this in your init.lua
vim.api.nvim_create_autocmd(
    {"BufEnter", "BufWinEnter"},
    {
        pattern = {"*.yuck"},
        callback = function(event)
            print(string.format("starting yuck;s for %s", vim.inspect(event)))
            vim.lsp.start {
                name = "YuckLs",
                cmd = {"dotnet", "/home/gitrepos/yuckls/YuckLS/dist/YuckLS.dll"}, --this must be where you cloned this repo to.
                --cmd = {"yuckls"} -- if installed from aur
                root_dir = vim.fn.getcwd()
            }
        end
    }
)
```

# Helix
```
# put this in your languages.toml
[language-server]
yuckls = {command="dotnet" , args = ["/home/gitrepos/yuckls/YuckLS/dist/YuckLS.dll"]} #must be where you cloned this repo to
#yuckls = {command = "yuckls } #if installed from aur
[[language]]
name="yuck"
scope="source.yuck"
injection-regex="yuck"
file-types=["yuck"]
language-servers = ["yuckls"]
```
## Custom widget autocompletion
For completions for custom widgets defined with (defwidget , your workspace must contain an eww.yuck file somewhere up the directory tree. The lsp will assume this file is the entry point for EWW. In this file, widgets defined will be available for completion. Yuck Source files that have been included into eww.yuck will loaded recursilvely for completion as well, this means imported yuck files and even yuck files imported into imported files will be parsed. Includes must be kept relative to your eww.yuck file. Basically just set it up according to EWW's docs and it should work.


https://github.com/user-attachments/assets/a587fae7-3c4c-49c3-bb83-88ed9af4f902




## ROADMAP
✔️ Basic in built type completions for widgets and properties

✔️ Basic Completions for custom widgets 

✔️ Basic Diagnostics and error reporting

❌ Go to definition for custom widgets and variables

❌ Buffer formatting  



https://github.com/user-attachments/assets/4beb085d-ae2d-4b65-acbd-17479d31cb03

