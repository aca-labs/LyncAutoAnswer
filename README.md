Lync 2013 Auto Answer
=============================
A simple tray application that turns on Lync 2013 auto-answer and automatically starts video and full-screen mode. Ideal for conference rooms or hands-off remote kiosk units.

![Tray Icon](readme-content/TrayIcon.png)

This is a fork of [Scott Hanselmans auto-answer](https://github.com/shanselman/LyncAutoAnswer), which has quite a few unhandled edge cases that are corrected in this version. For example, it will answer calls that are restablishing after a dropped connection.

Features:
* Auto-answers video calls
* Switches to full-screen mode after answering the call
* Continues to work even after a Lync restart
* Answers video for existing conversations (IM/Audio/etc)
* Windowless tray application
* Extensive logging

[Click here for the Trello board for this project](https://trello.com/board/lync-auto-answer/5194e27988957bcc70002c23)

Pro-tip: Disable video cropping (square video) for conference rooms so that you get a wide-screen view. To do this, add a registry key called "CropOutgoingVideo" to "HKCU\Software\Microsoft\Office\15.0\Lync", and set it the value to 0 for conference rooms. Make sure that you have the latest Lync 2013 updates to ensure this setting will apply.

[Fork by Jason Young](http://www.ytechie.com)