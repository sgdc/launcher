# SGDC Launcher 2.0
The SGDC Launcher 2.0 is a launcher designed to be the front-end experience for users of the Jacobus Arcade Machine. It has been rewritten from the ground up for Windows with Xbox 360 Controller Support, global controls (kill game, volume up/down/mute), play stats for games, and easily configured on-screen messages.


## SGDC Launcher Features

- Automatically populated game's list. (Uses alphabetical sort from /games folder, so rearrange in that manner)
- F1 system key: kill the running game process, guaranteed.
- F2 system key: Volume up.
- F3 system key: Volume down.
- F4 system key: Mute/Unmute.
- Video previews. Specified videos will loop upon that game being highlighted.
- Xbox 360 Controller and Keyboard support.
- ATTRACT MODE: After 30 seconds of inactivity, the machine will start automatically scrolling the game's list downwards. This cycles the game preview being shown and is meant to subtly catch the eye.
- Configurable. `config.json` in the root has info like the Subtitle underneath the SGDC logo, the info bar on the right's text, amount of time to activate ATTRACT MODE, etc.

## Game criteria

Technically speaking, any game that can run on Windows 10 will be able to run using this launcher on our machine, but we want to stress a few more things in the interest of maintaining a high quality product.
To be featured on the arcade machine, your game needs to:
- boot to full screen
- have an in-game quit function
- be self-explanatory (in-game tutorial or controls explanation)
- be compatible with Xbox 360 controllers (1-4 players)

## Adding games
The launcher source solution here on Github needs to be executed from Visual Studio (a release can be found [here](https://github.com/sgdc/launcher/releases)).

Once you have the launcher Exe, the folder should have some dlls, the exe, a /videos/ folder, and a /games/ folder. Video previews for games (any format compatible with Windows Media Player) should go in videos, and each game should have a unique folder in /games/.

### I have a game to add, what do I do?
First things first: does your game start in fullscreen and have controller support? Great! It can go on the machine. You'll need to make a small file called info.json in the directory of your game.

### Info.json?
It should look like this:
```
    {
        "name": "Game name",
        "description": "Game description.",
        "devs": "Person A and Person B",
        //below this is optional
        "videoName": "videoInVideosFolder.mp4",
        "exeName": "theSpecificFileToLoad.jar" //if this is not specified, the launcher looks for the first exe it can find in your folder.
    }
```

Once you've made that you should have a folder that looks like something this:
```
/myGameFolder ->
    /gameData
    game.exe
    info.json
    [any other needed files]
```

Put that folder into the /games/ folder, and start up the launcher. It should add your game to the list!

## Configuration
config.json is an optional config file that can let you tweak things about the launcher easily. It's a single json object that looks like this:
```
{
	"message": "The Jacobus Arcade Machine is presented by the Stevens Game Development Club.\n\nIf this machine is malfunctioning, or you have any questions, comments, or concerns please reach out to us at sgdc@stevens.edu\n\nLearn more about what we do at http://sgdc.ml",
	"subtitle": "Jacobus Arcade Machine",
	"attractActivate": 45000,
	"attractScroll": 10000,
	"startBlink": 850
}
```

attractActivate is the time (in ms) that the launcher needs to see no input to enter Attract Mode, which scrolls down every "attractScroll" ms.
startBlink is how fast the "Press Start" message should blink, just in case you want to speed it up or slow it down.
message and subtitle are self explanatory.

### logo
If a file called `logo.png` is placed in the launcher directory, that image will overwrite the SGDC logo in the top left corner. Handy to know!


## Errors
If the launcher doesn't boot up upon starting, something must not be configured correctly. error.txt should be created in the launcher directory, which may let you know what is wrong.
