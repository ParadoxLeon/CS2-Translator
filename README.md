# CS2-Translator ![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/ParadoxLeon/CS2-Translator/total?style=for-the-badge&logo=GitHub&color=%23663399)
Easy to use single file CS2 chat translation tool. 

You probably know the feeling of not being able to communicate in a match because everyone speaks a different language.
Well, gone are those days. With this tool you can read translations of all foreign language messages while staying in the game.

CS2-Translator will read the console output, detect chat messages, and then translate them with Google Translate.

All of this is accomplished using official CS2 launch options.

## How to use

1. Download the [latest Version](https://github.com/ParadoxLeon/CS2-Translator/releases) (or build it yourself)
2. Set CS2 launch options: `-condebug` ([how do I do this?](https://support.steampowered.com/kb_article.php?ref=1040-JWMT-2947)) 
3. start CS2 & CS2 Translator
4. (Optional) Change the options in CS2 Translator to your liking. ([List of language codes](https://cloud.google.com/translate/docs/languages))

### Your game needs to be set to English

## Update

To update the translator just download the [latest Version](https://github.com/ParadoxLeon/CS2-Translator/releases) (or build it yourself)
Make sure to delete the old one.

## Features & Examples

### Translations
![](img/translation.png)<br /><br />

### Options
![](img/options.png)<br /><br />

### Other features
* Very simple to use
* Single File
* Lots of languages supported ([Full list of language codes](https://cloud.google.com/translate/docs/languages))
* Translations are cached, so the same translation is used for identical messages.
* Will work with all CS2 install locations.
* Everything done from in the game, no need to Alt + Tab.
* New version notification

### Current limitations
* The current method of using Google Translate is rate-limited at 100 requests / hour.
* Can't detect messages on certain community servers because of different chat structures.

### What I changed:
- switched to Dark Mode
- removed options for telnet
- changed the default location
- added Text wrapping
