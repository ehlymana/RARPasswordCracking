Prerequisites:

- *WinRAR software* (can be downloaded from [here](https://www.win-rar.com/start.html?&L=0))
- *John the Ripper cracking tool* (can be downloaded from [here](https://www.openwall.com/john/))
- *Hashcat cracking tool* (can be downloaded from [here](https://hashcat.net/hashcat/))
- *Dictionaries* which will be used for dictionary attacks (can be downloaded from [here](https://github.com/danielmiessler/SecLists))
- *RockYou dataset* (can be downloaded from [here](https://www.kaggle.com/datasets/wjburns/common-password-list-rockyoutxt?resource=download))

You need to check the contents of the region **Global variables** in the **Program.cs** file of the *Console application*.
Global variables need to point to the locations of the aforementioned tools.

It is also necessary to create a file named *file.txt*. It can contain whatever you want and will be used for creating the desired RAR archives.

The application needs to be recompiled upon making changes. If no changes are necessary, you can start the application directly from:
> Console application/RandomPasswordGenerator/bin/Release/net6.0/RandomPasswordGenerator.exe.

Example of achieved results can be found at the **Results** folder.

This application has only been made public for the purpose of helping reviewers for an international conference and ***SHOULD NOT be reused or replicated in any way*** until the research is published.
