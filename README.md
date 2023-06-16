If you use this software tool for your research, please cite the following work:

```
E. Krupalija, S. Mrdović, E. Cogo, I. Prazina and Š. Bećirović, "Evaluation of the security of password-protected encrypted RAR3 and RAR5 archives," NCIT 2022; Proceedings of International Conference on Networks, Communications and Information Technology, Virtual, China, 2022, pp. 1-7, [doi: ](https://ieeexplore.ieee.org/document/10153749)
```

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
