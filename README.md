## ImageToXNB

Commandline utility to convert any GDI+ supported image (BMP, GIF, JPEG, PNG, TIFF) to XNB image. The version of XNB file will be Version 4 (XNA Framework 3.1). 
Mainly created to modify Defy Gravity -textures, but should work for all games created with XNA Framework 3.1 (and might work with XNA Framework 4.0, as it should be backwards compatible).

### Installation

Download the executable from releases. Extract to your chosen location.

### Usage

In commandline with ImageToXNB (or in the same directory), run

ImageToXNB <source> [<destination>]

Destination is optional, and in case it is omitted, the filename is the same with the extension replaced with "xnb".

