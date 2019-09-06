﻿//
var jcrop_api,
    boundx,
    boundy,
    xsize,
    ysize;

var jcrop_api2,
    boundx2,
    boundy2,
    xsize2,
    ysize2;

// ToDo - change the size limit of the file. You may need to change web.config if larger files are necessary.
var maxSizeAllowed = 4;     // Upload limit in MB
var maxSizeInBytes = maxSizeAllowed * 1024 * 1024;
var keepUploadBox = false;  // ToDo - Remove if you want to keep the upload box
var keepCropBox = false;    // ToDo - Remove if you want to keep the crop box

$(function () {
   
    if (typeof $('#avatar-upload-form') !== undefined) {
        initAvatarUpload();
        $('#avatar-max-size').html(maxSizeAllowed);
        $('#avatar-upload-form input:file').on("change", function (e) {
            var files = e.currentTarget.files;
            for (var x in files) {
                if (files[x].name != "item" && typeof files[x].name != "undefined") {
                    if (files[x].size <= maxSizeInBytes) {
                        // Submit the selected file
                        $('#avatar-upload-form .upload-file-notice').removeClass('bg-danger');
                        $('#avatar-upload-form').submit();
                    } else {
                        // File too large
                        $('#avatar-upload-form .upload-file-notice').addClass('bg-danger');
                    }
                }
            }
        });
    }
});

function initAvatarUpload() {
    
    $('#avatar-upload-form').ajaxForm({
        beforeSend: function () {
            updateProgress(0);
            $('#avatar-upload-form').addClass('hidden');
        },
        uploadProgress: function (event, position, total, percentComplete) {
            updateProgress(percentComplete);
        },
        success: function (data) {
            updateProgress(100);
            if (data.success === false) {
                $('#status').html(data.errorMessage);
            } else {
                $('#preview-pane .preview-container img').attr('src', data.fileName);
                var img = $('#crop-avatar-target');
                img.attr('src', data.fileName);

                if (!keepUploadBox) {
                    $('#avatar-upload-box').addClass('hidden');
                }
                $('#avatar-crop-box').removeClass('hidden');
                initAvatarCrop(img);

                $('#preview-pane2 .preview-container2 img').attr('src', data.fileName);
                var img2 = $('#crop-avatar-target2');
                img2.attr('src', data.fileName);
                initAvatarCrop2(img2);
            }
        },
        complete: function (xhr) {
        }
    });
}

function updateProgress(percentComplete) {
    $('.upload-percent-bar').width(percentComplete + '%');
    $('.upload-percent-value').html(percentComplete + '%');
    if (percentComplete === 0) {
        $('#upload-status').empty();
        $('.upload-progress').removeClass('hidden');
    }
}

function initAvatarCrop(img) {
    img.Jcrop({
        onChange: updatePreviewPane,
        onSelect: updatePreviewPane,
        aspectRatio: xsize * 1.5 / ysize
    }, function () {
        var bounds = this.getBounds();
        boundx = bounds[0];
        boundy = bounds[1];

        jcrop_api = this;
        jcrop_api.setOptions({ allowSelect: true });
        jcrop_api.setOptions({ allowMove: true });
        jcrop_api.setOptions({ allowResize: true });
        jcrop_api.setOptions({ aspectRatio: 1.5 });

        // Maximise initial selection around the centre of the image,
        // but leave enough space so that the boundaries are easily identified.
        var padding = 10;
        var shortEdge = (boundx < boundy ? boundx : boundy) - padding;
        var longEdge = boundx < boundy ? boundy : boundx;
        var xCoord = longEdge / 2 - shortEdge / 2;
        jcrop_api.animateTo([xCoord, padding, shortEdge, shortEdge]);

        var pcnt = $('#preview-pane .preview-container');
        xsize = pcnt.width();
        ysize = pcnt.height();
        $('#preview-pane').appendTo(jcrop_api.ui.holder);
        jcrop_api.focus();
    });
}

function updatePreviewPane(c) {
    if (parseInt(c.w) > 0) {
        var rx = xsize / c.w;
        var ry = ysize / c.h;

        $('#preview-pane .preview-container img').css({
            width: Math.round(rx * boundx) + 'px',
            height: Math.round(ry * boundy) + 'px',
            marginLeft: '-' + Math.round(rx * c.x) + 'px',
            marginTop: '-' + Math.round(ry * c.y) + 'px'
        });
    }
}


function initAvatarCrop2(img) {
    img.Jcrop({
        onChange: updatePreviewPane2,
        onSelect: updatePreviewPane2,
        aspectRatio: xsize2 / ysize2
    }, function () {
        var bounds = this.getBounds();
        boundx2 = bounds[0];
        boundy2 = bounds[1];

        jcrop_api2 = this;
        jcrop_api2.setOptions({ allowSelect: true });
        jcrop_api2.setOptions({ allowMove: true });
        jcrop_api2.setOptions({ allowResize: true });
        jcrop_api2.setOptions({ aspectRatio: 1 });

        // Maximise initial selection around the centre of the image,
        // but leave enough space so that the boundaries are easily identified.
        var padding = 10;
        var shortEdge = (boundx2 < boundy2 ? boundx2 : boundy2) - padding;
        var longEdge = boundx2 < boundy2 ? boundy2 : boundx2;
        var xCoord = longEdge / 2 - shortEdge / 2;
        jcrop_api2.animateTo([xCoord, padding, shortEdge, shortEdge]);

        var pcnt = $('#preview-pane2 .preview-container2');
        xsize2 = pcnt.width();
        ysize2 = pcnt.height();
        $('#preview-pane2').appendTo(jcrop_api2.ui.holder);
        jcrop_api2.focus();
    });
}

function updatePreviewPane2(c) {
    if (parseInt(c.w) > 0) {
        var rx = xsize2 / c.w;
        var ry = ysize2 / c.h;

        $('#preview-pane2 .preview-container2 img').css({
            width: Math.round(rx * boundx) + 'px',
            height: Math.round(ry * boundy) + 'px',
            marginLeft: '-' + Math.round(rx * c.x) + 'px',
            marginTop: '-' + Math.round(ry * c.y) + 'px'
        });
    }
}


function saveAvatar() {
    var img = $('#preview-pane .preview-container img');
    $('#avatar-crop-box button').addClass('disabled');

    $.ajax({
        type: "POST",
        url: "/Avatar/Save",
        traditional: true,
        data: {
            w: img.css('width'),
            h: img.css('height'),
            l: img.css('marginLeft'),
            t: img.css('marginTop'),
            fileName: img.attr('src')
        }
    }).done(function (data) {
        if (data.success === true) {
            $('#avatar-result img').attr('src', data.avatarFileLocation);

            $('#avatar-result').removeClass('hidden');

            if (!keepCropBox) {
                $('#avatar-crop-box').addClass('hidden');
            }
        } else {
            alert(data.errorMessage)
        }
    }).fail(function (e) {
        alert('Cannot upload avatar at this time');
    });
}


function saveAvatar2() {
    
    var img = $('#preview-pane .preview-container img');
    var img2 = $('#preview-pane2 .preview-container2 img');
    $('#avatar-crop-box button').addClass('disabled');

    $.ajax({
        type: "POST",
        url: "/Avatar/Save2",
        traditional: true,
        data: {
            w: img.css('width'),
            h: img.css('height'),
            l: img.css('marginLeft'),
            t: img.css('marginTop'),
            fileName: img.attr('src'),
            w2: img2.css('width'),
            h2: img2.css('height'),
            l2: img2.css('marginLeft'),
            t2: img2.css('marginTop')
        }
    }).done(function (data) {
        if (data.success === true) {
            $('#avatar-result img').attr('src', data.avatarFileLocation);

            $('#avatar-result').removeClass('hidden');

            if (!keepCropBox) {
                $('#avatar-crop-box').addClass('hidden');
            }
        } else {
            alert(data.errorMessage)
        }
    }).fail(function (e) {
        alert('Cannot upload avatar at this time');
    });
}