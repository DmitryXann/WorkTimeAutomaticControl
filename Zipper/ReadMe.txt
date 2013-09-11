CMD Line params:
0 - full path to directory
1 - zip archive name, for example: my.zip

Return code info:
0 - all ok, directory zipped
-1 - not enough params
-2 - directory not found
-3 - file name contains invalid chars
-4 - incorrect file name
-5 - IOException
-6 - File handling exception
-7 - zipping exception