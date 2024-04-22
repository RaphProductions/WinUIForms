# WinUIForms
NOTE: This project is now part of the new Project Cappuccino desktop. This repository is kept online, archived while the desktop isn't open source.

WinUIForms is a project to "convert WinForms forms to WinUI 3/WASDK forms".

## How it works
WinUIForms works by calling a function at app startup, which will do the "conversion" process:

Create a WinUI 3 window, with a content grid inside

Loop over all the form's controls:
For each control, check if there's an available converter:

If there's one, pass the control to the converter, which will "convert" the WinForm control to a WinUI 3 control, and so add it to the WinUI content grid.
Else, print a warning in the debugger and skip the control.


WinUIForms use converters, a piece of code which takes a WinForms control (example: System.Windows.Forms.Button), and convert it to it's WinUI 3 equivalent (example: Microsoft.UI.Xaml.Controls.Button)

## How to compile the test app?
* Clone the source code and open the solution in your favourite IDE (Visual Studio, Rider...)
* Click on the Run button of your IDE
