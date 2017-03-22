var gulp = require('gulp');
var dotCover = requireModule('gulp-dotnetcover');
gulp.task('cover-dotnet', function() {
    return gulp.src('**/*.Tests.dll')
             .pipe(dotCover({
                 debug: false,
                 architecture: 'x86',
                 exclude: ['FluentMigrator.*', 
                            'AutoMapper',
                            'AutoMapper.*']
             }));
});

