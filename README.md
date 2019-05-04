# Local-multiplayer-UI
A unity project made for navigating the UI in a local multiplayer game. 

I'm working on my own game [Setting Sun](https://www.indiedb.com/games/setting-sun) which contains local coop and I needed an interface that can be used by multiple players.

It wasn't easy to find any info on how to make a coop interface work in unity.
I finally figured it out and wanted to put the project up for everyone struggling with this.

The project contains a custom input module and custom event system made for 4 controllers using X_input/direct input.

It uses the built-in unity event system so implementing should be easy.

There's a premade example scene called Coop UI navigation where 4 xbox controllers can each navigate their own panel with buttons.

There's a toggle on the player input for both X_Input and unity's direct input.

Unity's input manager is also configured for 4 controllers (all though that's not saying much cause I pretty much had no choice but to use X_Input)

How to use:
- To make this work you need a game object for each player containing the X_input module and the Custom event system.
The original standalone input module and event system can be disabled/removed.

- Each player should have the Player Input script which contains the code to read the button input.
The player input script has a public field called controller ID. 

This ID needs to correspond to the controller ID from Xinput or the number you've given to the names in unity's input manager. (you might have to tinker with the code depending on your names)

- Each player input script then needs to be assigned to a X_Input module and the custom event system needs a button assigned to be the first selected button for each UI panel.

Notes:
- When creating a panel with buttons, you can't have the navigation on Automatic because this auto connects all the buttons allowing players to navigate to other player's UI.
This wouldn't be such a problem if automatic would only work per gameobject they are contained in, but it don't.
If you spawn buttons dynamically you'll have to code the connections explicitly.

- There's a player manager script which can be used to see which controllers are connected and contains code for reassigning controllers when plugging/unplugging.
It can also be used to spawn players and then manually assign the controllerID/script to the player input and the input module.

