var gulp = require("gulp");
var browserify = require("gulp-browserify");
var react = require("gulp-react");
var uglify = require("gulp-uglify");
var del = require("del");

var paths = {
    scripts: ["src/**/*.js"],
    components: ["src/**/*.jsx"],
    css: ["src/**/*.css"],
    html: ["src/**/*.html"],

    builtTemp: "build/temp",

    outputScripts: "build/out/scripts",

    outputDir: "build/out",
};

gulp.task('clean', function (cb) {
    del(['build'], cb);
});

gulp.task("copyHtml", function () {
    return gulp.src(paths.html)
        .pipe(gulp.dest(paths.outputDir));
});

gulp.task("copyHtml", function () {
    return gulp.src(paths.html)
        .pipe(gulp.dest(paths.outputDir));
});

gulp.task("copyCss", function () {
    return gulp.src(paths.css)
        .pipe(gulp.dest(paths.outputDir));
});

gulp.task("components", function () {
    return gulp.src(paths.components)
        .pipe(react())
        .pipe(gulp.dest("src"));
});

gulp.task("scripts", function () {
    return gulp.src(paths.scripts)
        .pipe(gulp.dest(paths.builtTemp));
});

gulp.task("combineScripts", ["scripts", "components"], function () {
    return gulp.src(["build/temp/scripts/app.js", "build/temp/scripts/react-0.11.js"])
        .pipe(browserify())
        .pipe(uglify())
        .pipe(gulp.dest(paths.outputDir));
});

gulp.task("styles", function () {
    return gulp.src(paths.components)
        .pipe(gulp.dest())
});

gulp.task("default", ["copyHtml", "copyCss", "components", "scripts"]);