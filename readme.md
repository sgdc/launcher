# SGDC Launcher 2.6
The SGDC Launcher 2.6 is a launcher designed to be the front-end experience for users of the Jacobus Arcade Machine. It has been rewritten from the ground up for Windows with Xbox 360 Controller Support, global controls (kill game, volume up/down/mute), play stats for games, and easily configured on-screen messages.


## SGDC Launcher Features

- Automatically populated game's list. (Uses alphabetical sort from the `games` folder, so rearrange in that manner)
- F1 system key: kill the running game process, guaranteed.
    - By default, holding Back+Select on an Xbox Controller for 1 second will accomplish this as well.
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
- be crash resistant (or at least have the response to an error be to instantly close)

## Adding games
The launcher here on Github needs to be executed from Visual Studio (a release can be found under Releases).

Once you have the launcher Exe, the folder should have some dlls, the exe, and a /games/ folder. Each game should have a unique folder in /games/, and in each folder you should have an info.json and a .webm video preview. You can look up online video converters to convert from mp4 files to webm, or look below for ffmpeg commands to do the same.

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
        "exeName": "theSpecificFileToLoad.jar" //if this is not specified, the launcher looks for the first .exe it can find in the game's folder. This field is necessary if the game is not a .exe file.
    }
```

Once you've made that you should have a folder that looks like something this:
```
/myGameFolder ->
    /gameData
    game.exe
    info.json
    video.webm (technically optional)
    [any other needed files]
```

Put that folder into the /games/ folder, and start up the launcher. It should add your game to the list!

## Configuration
config.json is an optional config file that can let you tweak things about the launcher easily. It's a single json object that looks like this:
```
{
	"message": "The Jacobus Arcade Machine is presented by the Stevens Game Development Club.\n\nIf this machine is malfunctioning, or you have any questions, comments, or concerns please reach out to us at sgdc@stevens.edu\n\nLearn more about what we do at http://sgdc.dev",
	"subtitle": "Jacobus Arcade Machine",
	"attractActivate": 45000,
	"attractScroll": 10000,
	"startBlink": 850,
    "borderless": false,
    "lockMouse": false,
    "closeWithBackAndStart": true
}
```

attractActivate is the time (in ms) that the launcher needs to see no input to enter Attract Mode, which scrolls down every "attractScroll" ms.

startBlink is how fast the "Press Start" message should blink, just in case you want to speed it up or slow it down. (Note: Deprecated as of 2.5)

message and subtitle are self explanatory.

borderless being set to true will see the launcher startup in borderless fullscreen mode. This should be set to true for the machine proper, but likely false for users running the launcher on their own machines.

lockMouse being set to true will see the launcher force the mouse into the bottom right corner of the screen, keeping it out of the way. This should be true for the machine proper, but probably false otherwise.

closeWithBackAndStart is true by default, and enables a macro where Xbox controllers can hold Back+Start for a second to force quit an open game.

### logo
If a file called `logo.png` is placed in the launcher directory, that image will overwrite the SGDC logo in the top left corner. Handy to know!


## Errors
If the launcher doesn't boot up upon starting, something must not be configured correctly. error.txt should be created in the launcher directory, which may let you know what is wrong.

## Regarding .webm
This launcher is now running an embedded Chromium window inside a Winforms application. (Holy bizarre tech stacks). Chromium does not support .mp4, which is unfortunate. It does support .webm, which MP4 can be converted to fairly easy using online [converter sites](https://cloudconvert.com/webm-converter), or through the following esoteric [FFMPEG](https://ffmpeg.org/download.html) command:
(To convert input.mp4 to output.webm. Change these names if desired.):
`./ffmpeg.exe -i ./input.mp4 -c:v libvpx-vp9 -crf 30 -b:v 0 -b:a 128k -c:a libopus output.webm`

## Other Notes
- The Launcher keeps track of "number of times game launched" and "time game has spent open" on a per game basis, and writes those values to each game's `info.json`
- The primary reason this Launcher was written as a Winforms application was so that we could make use of Windows hooks allowing us to capture the F1, F2, F3, and F4 keys regardless of what program was focused, along with being able to launch child processes we could later force to quit. We have also added support for controllers holding Start+Back to force quit games.
- The reason we embedded a Chromium window into the program was because Winforms is hard to make look nice, and extremely hard to make responsive to different sizes. A 100% scaled web window solved these issues, after we found a pipeline to beam information down to the HTML file.
- A number of the essentially required files (the _index.html, logo.png, style.css, the static.webm and loadinggame.webm videos, the default info.json, and probably more) are not properly bundled in the project as assets. If starting from scratch, Start the project in Debug mode once to generate a `/bin/Debug` folder, then copy the contents of the `/Copy Into Debug or Release/` folder in the project root into that `/bin/Debug/` folder. Once you run the project a second time, it should fully start up correctly.
- Honestly if you want to make modifications to this project you'll probably want to reach out to me. I'm in the SGDC Discord and you can also find me through the commit history of this repo.