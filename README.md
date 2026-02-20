# Git change log generator

Simple git changelog generator in C#

&nbsp;&nbsp;**ChangeLogGenerator --txt | --md | --rtf | --html [--nocredit]  [--repo path] [--branch name] [--output outfile]** 

```
% ChangeLogGenerator --rtf --output ChangeLog.rtf
```
#### <span style="background-color:rgb(0,100,0);color:rgb(255,255,255)">1.2.2</span>
**16 October 2023**
- Update LibGit2Sharp
- Outfile extension validation
#### <span style="background-color:rgb(0,100,0);color:rgb(255,255,255)">1.2.1</span>
**15 September 2023**
- Carriage return in message fixed on rtf
- Arg checking
- Colour console output
- -untagged arg. Improve formatting
- Update LibGit2Sharp for Ubuntu-20.04 support
#### <span style="background-color:rgb(0,100,0);color:rgb(255,255,255)">1.2.0</span>
**30 March 2023**
- DotNet7.0
- Optimise rtf output, fix text duplicates
- Support Rtf output. -repo path argument
#### <span style="background-color:rgb(0,100,0);color:rgb(255,255,255)">1.1.0</span>
**11 March 2023**
- Switch to LibGit2Sharp
- Add usage message. Refactor
- Better formatted md / html
- Simplify regex
- Refactor tests
#### <span style="background-color:rgb(0,100,0);color:rgb(255,255,255)">1.0.0</span>
**01 March 2023**
- Improved 'tag' detection
- Initial project files

