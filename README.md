# WinUserMigrationTool
Copies user data + restores user data.

------------------------------------------------------------------------------------------
COPY:

- User folder (pictures, downloads, Music, favorites, documents, desktop, videos)
Status:
GetAllNotHiddenUsers - function created. Lists all not hidden user folders under whatever path was given as parameter. Now more generalized.
CopyPasteUser - function created. Copies files from given source dir (string) and places them into given target dir(string).
PopulateUserFolderListbox - function created. Populates listbox/view with not hidden user folders. Now more generalized, can be used to populate both listviews.

- network drives/folders
Status:
GetNetworkDrives - fetches all available network drives and returns their unc paths. (work in progress)
MapNetworkDrives - remaps fetched network drives.(work in progress)
SaveUncsToConfig - Saves fetched network drive paths to conf under "paths" key.

- printers / network printers
Status:


- browser bookmarks
Status:


- browser saved passwords
Status:


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


- restore copied browser bookmarks and passwords
Status:


------------------------------------------------------------------------------------------

OTHER:

- Launchable from usb (no installation)
Status:


- UI lists all users that can be copied and or restored.
Status:


- UI lists all user folders too (maybe)
Status:


- Easy to use copy/restore
Status:


- user data is copied to the root of the usb into a folder named after the user (copied user)
Status:


------------------------------------------------------------------------------------------