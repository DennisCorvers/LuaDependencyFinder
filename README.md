# LuaDependencyFinder

This application scans provided .Lua files and searches for required dependencies based on the MediaWiki Scribuntu Extension. It scans files for the "require" keyword and downloads all missing modules recursively. The "Module:" prefix is also stripped from the .lua files, as this prefix isn't valid as a directory name.

External dependencies such as the mw libraries are not automatically retrieved.

There is also a functionality to update the already downloaded files, if the targeted Mediawiki has newer revisions.


For the application to function properly, the generated configuration file needs to be filled out with the targeted Mediawiki API information. This can be found on the specific Mediawiki on the Special:Version page.
