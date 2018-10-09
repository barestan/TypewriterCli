# Typewriter Cli
Typewriter NetCore version with command line interface and single file processing.  

## Restrictions
- Single file processing  
Code model (classes, properties, methods etc) returns only objects of processing file.
- Type.IsDefined is not supported (stub)   
Type.IsDefined always returns Type.!IsPrimitive.  
- Settings.IncludeCurrentProject() and Settings.IncludeProject("Project.Name") is not supported (stub).
Use Rider `File Watchers` settings to select projects.
