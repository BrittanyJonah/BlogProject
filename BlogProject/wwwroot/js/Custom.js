

let index = 0;

function AddTag() {
    var tagEntry = document.getElementById("TagEntry");

    //use search function to detect errors
    let searchResult = Search(tagEntry.value);

    if (searchResult != null) {

        //trigger sweetalert for error string
        swalWithBlueButton.fire({
            icon: 'error',
            html: `${searchResult}`,
        });
    }
    else {
        //create a new select option
        let newOption = new Option(tagEntry.value, tagEntry.value);

        document.getElementById("TagList").options[index++] = newOption;
    }

    //Clear out tag textbox
    tagEntry.value = "";
    return true;
}

function RemoveTag() {
    let tagCount = 1;

    let tagList = document.getElementById("TagList");

    if (!tagList) return false;
    if (tagList.selectedIndex == -1) {
        swalWithBlueButton.fire({
            icon: 'error',
            html: `Please choose a tag to remove.`,
        });
        return true;
    }

    while (tagCount > 0) {
        if (tagList.selectedIndex >= 0) {
            tagList.options[tagList.selectedIndex] = null;
            --tagCount;
        }
        else 
            tagCount = 0;
        index--;
    }
}

$("form").on("submit", function () {
    $("#TagList option").prop("selected", "selected");
})

//look for the tagValues var to see if it has data
if (tagValues != '') {
    let tagArray = tagValues.split(",");
    for (let loop = 0; loop < tagArray.length; loop++) {
        //Load up or replace the options that we have
        ReplaceTag(tagArray[loop], loop);
        index++;
    }
}

function ReplaceTag(tag, index) {
    let newOption = new Option(tag, tag);
    document.getElementById("TagList").options[index] = newOption;
}

//The search function detects empty or duplicate tags on the same post
function Search(str) {
    if (str == "") {
        return 'Empty tags are not permitted.';
    }
    var tagsEl = document.getElementById("TagList");
    if (tagsEl) {
        let options = tagsEl.options;
        for (let index = 0; index < options.length; index++) {
            if (options[index].value == str){
                return `The tag #${str} already exists.`;
            }
        }
    }
}

const swalWithBlueButton = Swal.mixin({
    customClass: {
        confirmButton: "btn btn-primary"
    },
    buttonsStyling: false
})
