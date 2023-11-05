# WinUserMigrationTool
Copies user data + restores user data.

------------------------------------------------------------------------------------------
COPY:

- User folder (pictures, downloads, Music, favorites, documents, desktop, videos, and AppData\\Local\\Google\\Chrome\\User Data\\Default)
Status:
GetAllNotHiddenUsers - function created. Lists all not hidden user folders under whatever path was given as parameter. Now more generalized.
CopyPasteUser - function created. Copies files from given source dir (string) and places them into given target dir(string) Things to copy.
PopulateUserFolderListbox - function created. Populates listbox/view with not hidden user folders. Now more generalized, can be used to populate both listviews.

- network drives/folders
Status:
GetNetworkDrives - fetches all available network drives and returns their unc paths. (work in progress)
MapNetworkDrives - remaps fetched network drives.(work in progress)
SaveUncsToConfig - Saves fetched network drive paths to conf under "paths" key.

- printers / network printers
Status:
GetPrinters - Returns names of all network/local printers, depending on the option chosen.
InstallLocalPrinters - Accepts list of printer names as a parameter and installs them.

- browser bookmarks
Status:
CopyPasteUser - Now handles chrome bookmark copying also.

- get outlook signatures
Status:


------------------------------------------------------------------------------------------

RESTORE:

- restore copied user folders
Status:
RestoreUserButton - Restores copied selected users from removable drive to local pc. (CopyPasteUser function is used in reverse here kinda)

- install network folders and printers
Status:


- restore copied outlook signatures
Status:


- restore copied browser bookmarks
Status:


------------------------------------------------------------------------------------------

OTHER:

- Launchable from usb (no installation)
Status:
Yes

- UI lists all users that can be copied and or restored.
Status:
Done but lists are updated only manually by pressing a button (for now)

- UI lists all user folders too (maybe)
Status:
Does not list hidden folders. App fetches all non hidden folders under "Users".

- Easy to use copy/restore
Status:
I guess yea. Lots of room for improvement tho.

- user data is copied to the root of the usb into a folder named after the user (copied user)
Status:
Done bit differently. User is copied to the root yes but under "CopiedUsers" - folder. Bit more organized this way i think.

------------------------------------------------------------------------------------------