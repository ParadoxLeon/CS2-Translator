# THIS IS A FORK (WIP)

# TODO

~~ - Dark Mode ~~
- Remove or reimplement telnet
~~ - Change the default location to fit CS2 ~~
- Change the README to fit the "new" application
- maybe change translation api to use DeepL instead of google
~~ - Change some strings to fit CS2 ~~
- maybe add language detection
~~ - change namespace to CsTranslator ~~
~~ - remove every CS:GO reverence ~~

# CS2-Translator
Easy to use, single-file CS2 chat translation tool. 

You probably know the feeling of not being able to communicate in a match because everyone speaks a different language.
Well, gone are those days. With this tool you can read translations of all foreign language messages while staying in the game.

CS2-Translator will read the console output, detect chat messages, and then translate them with Google Translate.

All of this is accomplished using official CS2 launch options.

## How to use

1. Download the [latest release](https://github.com/NiekNijland/CSGO-Translator/releases) (or build it yourself)
2. Set CS2 launch options: `-condebug` ([how do I do this?](https://support.steampowered.com/kb_article.php?ref=1040-JWMT-2947)) 
3. start CS2 & CS2-Translator
4. (Optional) Change the options in CS2Translator to your liking. ([List of language codes](https://cloud.google.com/translate/docs/languages))

## Features & Examples

### Translations
![](img/translations-in-app.png)<br /><br />

### Options
![](img/options.png)<br /><br />

### Other features
* Very simple to use
* Lots of languages supported ([Full list of language codes](https://cloud.google.com/translate/docs/languages))
* Translations are cached, so the same translation is used for identical messages.
* Will work with all CS2 install locations.
* Everything can be done from the game, no need to Alt + Tab.

### Current limitations
* The current method of using Google Translate is rate-limited at 100 requests / hour.
* Can't detect messages on certain community servers because of different chat structures.
