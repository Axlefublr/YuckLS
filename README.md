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
```lua
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
```toml
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

## ROADMAP
✔️ Basic in built type completions for widgets and properties

✔️ Basic Completions for custom widgets 

✔️ Basic Diagnostics and error reporting

❌ Go to definition for custom widgets and variables

❌ Buffer formatting  

# Limitations
For some reason i already regret, i decided to write the parser with just regex and string parsing instead of more standard methods like using lexers or ASTs. It's quirky but should work fine for many simple cases. Perhaps in the future or until a better solution is available, i might rewrite the whole thing to use ANTLR or something more flexible and concise.



![Screenshot_29-Dec_19-11-31_29472](https://github.com/user-attachments/assets/e4cb5a39-8692-42f6-8906-f9afe8201f30)


