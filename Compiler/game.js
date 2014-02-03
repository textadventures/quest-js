var selectSizeWithoutStatus = 8;
var selectSizeWithStatus = 6;
var numCommands = 0;
var thisCommand = 0;
var commandsList = new Array();
var tmrTick = null;
var tickCount = 0;
var sendNextGameTickerAfter = 0;
var verbButtonCount = 9;
var commandLog = null;

function init() {
    showStatusVisible(false);

    $("#button-restart").button().click(function () {
        $("#button-restart").removeClass("ui-state-focus ui-state-hover");
        uiDoRestart();
    });
    $("#button-undo").button().click(function () {
        $("#button-undo").removeClass("ui-state-focus ui-state-hover");
        uiDoUndo();
    });
    $("#button-wait").button().click(function () {
        $("#button-wait").removeClass("ui-state-focus ui-state-hover");
        uiDoWait();
    });
    //%%DEBUG START
    $("#button-test").button().click(function () {
        $("#button-test").removeClass("ui-state-focus ui-state-hover");
        reportError("test error");
    });
    //%%DEBUG END
    $("#button-options").button().click(function () {
        $("#button-options").removeClass("ui-state-focus ui-state-hover");
        $("#gameMore").hide();
        $("#gameOptions").show();
    });
    $("#fontOptions").change(function () {
        var newFont = $("#fontOptions option:selected").text();
        $("#divOutput div span").css("font-family", newFont);
        $("#fontSample").css("font-family", newFont);
        currentFont = newFont;
        set(GetObject("game"), "defaultfont", newFont);
        saveGame();
    });
    $("#fontSize").change(function () {
        var newFontSize = $("#fontSize option:selected").val();
        $("#divOutput div span").css("font-size", newFontSize + "pt");
        $("#fontSample").css("font-size", newFontSize + "pt");
        currentFontSize = newFontSize;
        set(GetObject("game"), "defaultfontsize", parseInt(newFontSize));
        saveGame();
    });

    $(document).on("click", ".elementmenu", function (event) {
        if (!$(this).hasClass("disabled")) {
            event.preventDefault();
            event.stopPropagation();
            // TO DO
            $(this).blur();
            return false;
        }
    });

    worldmodelInitialise();
    if (!loadGame()) {
        worldModelBeginGame();
    }
}

function extLink(url) {
    window.open(url, "_system");
}

function showStatusVisible(visible) {
    if (visible) {
        $("#statusVars").show();
        $("#statusLabel").show();
    }
    else {
        $("#statusVars").hide();
        $("#statusLabel").hide();
    }
}

var beginningOfCurrentTurnScrollPosition = 0;
var scrollTimeout = null;

function scrollToEnd() {
    if (scrollTimeout != null) {
        clearTimeout(scrollTimeout);
    }

    scrollTimeout = setTimeout(function () {
        scrollTimeout = null;
        scrollToEndNow();
    }, 200);
}

function scrollToEndNow() {
    $('html, body').animate({ scrollTop: beginningOfCurrentTurnScrollPosition - 30 }, 200);
}

function updateLocation(text) {
}

var _waitMode = false;
var _pauseMode = false;
var _waitingForSoundToFinish = false;

var waitButtonId = 0;

function beginWait() {
    if (runningWalkthrough) {
        awaitingCallback = false;
        waitCallback();
        TryFinishTurn();
        return;
    }
    _waitMode = true;
    waitButtonId++;
    addText("<a class=\"cmdlink\" style=\"color:" + currentLinkForeground + ";font-family:" + currentFont + ";font-size:" + currentFontSize + "pt;\" id=\"waitButton" + waitButtonId + "\" >Continue...</a><br/><br/>");
    $("#waitButton" + waitButtonId).click(function () {
        _waitMode = false;
        $(this).hide();
        $("#divCommand").show();
        beginningOfCurrentTurnScrollPosition = $("#gameContent").height();
        window.setTimeout(function () {
            awaitingCallback = false;
            waitCallback();
            TryFinishTurn();
        }, 100);
    });
    $("#divCommand").hide();
}

function beginPause(ms) {
    _pauseMode = true;
    $("#divCommand").hide();
    window.setTimeout(function () {
        endPause()
    }, ms);
}

function endPause() {
    _pauseMode = false;
    $("#divCommand").show();
    window.setTimeout(function () {
        // TO DO
        //$("#fldUIMsg").val("endpause");
        //$("#cmdSubmit").click();
    }, 100);
}

function globalKey(e) {
    if (_waitMode) {
        endWait();
        return;
    }
}

function commandKey(e) {
    switch (keyPressCode(e)) {
        case 13:
            runCommand();
            return false;
        case 38:
            thisCommand--;
            if (thisCommand == 0) thisCommand = numCommands;
            $("#txtCommand").val(commandsList[thisCommand]);
            break;
        case 40:
            thisCommand++;
            if (thisCommand > numCommands) thisCommand = 1;
            $("#txtCommand").val(commandsList[thisCommand]);
            break;
        case 27:
            thisCommand = numCommands + 1;
            $("#txtCommand").val("");
            break;
    }
}

function runCommand() {
    var command = $("#txtCommand").val();
    if (command.length > 0) {
        numCommands++;
        commandsList[numCommands] = command;
        thisCommand = numCommands + 1;
        sendCommand(command);
        $("#txtCommand").val("");
    }
}

function prepareCommand(command) {
    // TO DO
    //$("#fldUITickCount").val(getTickCountAndStopTimer());
    //$("#fldUIMsg").val("command " + command);
}

function showQuestion(title) {
    $("#msgboxCaption").html(title);

    var msgboxOptions = {
        modal: true,
        autoOpen: false,
        buttons: [
            {
                text: "Yes",
                click: function () { msgboxSubmit("yes"); }
            },
            {
                text: "No",
                click: function () { msgboxSubmit("no"); }
            }
        ],
        closeOnEscape: false,
        open: function (event, ui) { $(".ui-dialog-titlebar-close").hide(); }    // suppresses "close" button
    };

    $("#msgbox").dialog(msgboxOptions);
    $("#msgbox").dialog("open");
}

function msgboxSubmit(text) {
    $("#msgbox").dialog("close");
    window.setTimeout(function () {
        // TO DO
        //$("#fldUIMsg").val("msgbox " + text);
        //$("#cmdSubmit").click();
    }, 100);
}

var _menuSelection = "";

function showMenu(title, options, allowCancel) {
    $("#dialogOptions").empty();
    $.each(options, function (key, value) {
        $("#dialogOptions").append(
            $("<option/>").attr("value", key).text(value)
        );
    });

    $("#dialogCaption").html(title);

    var dialogOptions = {
        modal: true,
        autoOpen: false,
        buttons: [{
            text: "Select",
            click: function () { dialogSelect(); }
        }]
    };

    if (allowCancel) {
        dialogOptions.buttons = dialogOptions.buttons.concat([{
            text: "$$TEXT_CANCEL$$",
            click: function () { dialogCancel(); }
        }]);
        dialogOptions.close = function (event, ui) { dialogClose(); };
    }
    else {
        dialogOptions.closeOnEscape = false;
        dialogOptions.open = function (event, ui) { $(".ui-dialog-titlebar-close").hide(); };    // suppresses "close" button
    }

    _menuSelection = "";
    $("#dialog").dialog(dialogOptions);

    $("#dialog").dialog("open");
}

function dialogSelect() {
    _menuSelection = $("#dialogOptions").val();
    if (_menuSelection.length > 0) {
        $("#dialog").dialog("close");
        window.setTimeout(function () {
            SetMenuSelection(_menuSelection);
            updateLists();
        }, 100);
    }
}

function dialogCancel() {
    $("#dialog").dialog("close");
}

function dialogClose() {
    if (_menuSelection.length == 0) {
        dialogSendCancel();
    }
}

function dialogSendCancel() {
    window.setTimeout(function () {
        // TO DO
        //$("#fldUIMsg").val("choicecancel");
        //$("#cmdSubmit").click();
    }, 100);
}

function sessionTimeout() {
    disableInterface();
}

function gameFinished() {
    disableInterface();
}

function disableInterface() {
    $("#divCommand").hide();
    $("#gamePanesRunning").hide();
    $("#gamePanesFinished").show();
}

function playWav(filename, sync, looped) {
}

function playMp3(filename, sync, looped) {
    playAudio(filename, "mp3", sync, looped);
}

function playAudio(filename, format, sync, looped) {
}

function stopAudio() {
}

function finishSync() {
    _waitingForSoundToFinish = false;
    window.setTimeout(function () {
        $("#divCommand").show();
        $("#fldUIMsg").val("endwait");
        $("#cmdSubmit").click();
    }, 100);
}

function panesVisible(visible) {
    if (visible) {
        $("#gamePanes").show();
    }
    else {
        $("#gamePanes").hide();
    }
}

function uiShow(element) {
    if (element == "") return;
    $(element).show();
}

function uiHide(element) {
    if (element == "") return;
    $(element).hide();
}

var _compassDirs = ["northwest", "north", "northeast", "west", "east", "southwest", "south", "southeast", "up", "down", "in", "out"];

var lastPaneLinkId = 0;

function updateList(listName, listData) {
    var listElement = "";
    var emptyListLabel = "";

    if (listName == "inventory") {
        listElement = "#inventoryList";
        emptyListLabel = "#inventoryEmpty";
    }

    if (listName == "placesobjects") {
        listElement = "#objectsList";
        emptyListLabel = "#placesObjectsEmpty";
    }

    $(listElement).empty();
    $(listElement).show();
    var listcount = 0;
    var anyItem = false;

    $.each(listData, function (key, value) {
        var splitString = value.split(":");
        var objectDisplayName = splitString[0];
        var objectVerbs = splitString[1];

        if (listName == "inventory" || $.inArray(objectDisplayName, _compassDirs) == -1) {
            listcount++;
            lastPaneLinkId++;
            var paneLinkId = "paneLink" + lastPaneLinkId;
            $(listElement).append(
                "<li id=\"" + paneLinkId + "\" href=\"#\">" + objectDisplayName + "</li>"
            );
            bindMenu(paneLinkId, objectVerbs, objectDisplayName, false);
            anyItem = true;
        }
    });
    $(listElement + " li:last-child").addClass('last-child')
    if (listcount == 0) $(listElement).hide();
    if (anyItem) {
        $(emptyListLabel).hide();
    }
    else {
        $(emptyListLabel).show();
    }
}

function updateCompass(directions) {
    updateDir(directions, "NW", _compassDirs[0]);
    updateDir(directions, "N", _compassDirs[1]);
    updateDir(directions, "NE", _compassDirs[2]);
    updateDir(directions, "W", _compassDirs[3]);
    updateDir(directions, "E", _compassDirs[4]);
    updateDir(directions, "SW", _compassDirs[5]);
    updateDir(directions, "S", _compassDirs[6]);
    updateDir(directions, "SE", _compassDirs[7]);
    updateDir(directions, "U", _compassDirs[8]);
    updateDir(directions, "D", _compassDirs[9]);
    updateDir(directions, "In", _compassDirs[10]);
    updateDir(directions, "Out", _compassDirs[11]);
}

function updateDir(directions, label, dir) {
    $("#cmdCompass" + label).attr("disabled", $.inArray(dir, directions) == -1);
}

function compassClick(direction) {
    sendCommand(direction);
}

function sendCommand(text) {
    if (!gameRunning) return;
    if (awaitingInputCallback) {
        awaitingInputCallback = false;
        awaitingCallback = false;
        getinputCallback(text);
        return;
    }
    if (awaitingCallback) return;
    beginningOfCurrentTurnScrollPosition = $("#gameContent").height();

    if (_pauseMode || _waitingForSoundToFinish) return;
    if (_waitMode) {
        endWait();
        return;
    }
    window.setTimeout(function () {
        // TO DO - send tick count
        //prepareCommand(text);

        //%%MAX V510
        msg("");
        msg("&gt; " + text);
        //%%END MAX V510

        //%%DEBUG START
        if (text.substring(0, 4) == "dbg ") {
            runDebugCommand(text.substring(4));
        }
        else {
            //%%DEBUG END
            if (text.substring(0, 6) == "cheat ") {
                runCheatCode(text.substring(6));
            }
            else {
                sendCommandInternal(text);
            }
            //%%DEBUG START
        }
        //%%DEBUG END
    }, 100);
}

function sendCommandInternal(command) {
    var start = (new Date).getTime();
    addToCommandLog(command);
    HandleCommand(command);
    var diff = (new Date).getTime() - start;
    TryFinishTurn();
}

function addToCommandLog(command) {
    if (commandLog == null) {
        commandLog = new Array();
    }
    commandLog.push(command);
}

function runCheatCode(code) {
    var walkthrough = window["object_main"];
    if (walkthrough.steps.indexOf("label:" + code) > -1) {
        runWalkthrough("main", 0, 0, code);
    }
    else {
        sendCommandInternal("cheat " + code);
    }
}

//%%DEBUG START
function runDebugCommand(cmd) {
    msg("Debug command: " + cmd);
    if (cmd.substring(0, 2) == "w ") {
        walkthroughUndoTest = false;
        runWalkthrough(cmd.substring(2), 0, 0, "");
    }
    if (cmd.substring(0, 3) == "wu ") {
        walkthroughUndoTest = true;
        walkthroughUndoSteps = 100;
        runWalkthrough(cmd.substring(3), 0, 0, "");
    }
    if (cmd.substring(0, 3) == "wm ") {
        walkthroughUndoTest = false;
        var args = cmd.substring(3).split(" ", 2);
        runWalkthrough(args[1], 0, parseInt(args[0]), "");
    }
    if (cmd.substring(0, 3) == "wr ") {
        walkthroughUndoTest = false;
        var args = cmd.substring(3).split(" ", 3);
        runWalkthrough(args[2], parseInt(args[0]), parseInt(args[1]), "");
    }
    if (cmd == "log") {
        generateSaveLog(function (object, attribute, value) {
            msg(object.name + "." + attribute + "=" + value);
        });
    }
    if (cmd == "parent") {
        for (var idx in allObjects) {
            var obj = allObjects[idx];
            if (obj["_children"]) {
                var childList = "";
                for (var childIdx in obj["_children"]) {
                    childList += obj["_children"][childIdx].name + ",";
                }
                msg(obj.name + ": " + childList);
            }
            else {
                msg(obj.name + ": no children");
            }
        }
    }
}
//%%DEBUG END

function generateSaveLog(fn) {
    var gameElementArray = new Array();
    gameElementArray.push(GetObject("game"));
    generateSaveLogForArray(gameElementArray, fn);
    generateSaveLogForArray(allObjects, fn);
    generateSaveLogForArray(allExits, fn);
    generateSaveLogForArray(allCommands, fn);
    generateSaveLogForArray(allTurnScripts, fn);
    generateSaveLogForArray(allTimers, fn);
    thisTurnModifiedItems = new Array();
}

function generateSaveLogForArray(array, fn) {
    for (var idx in array) {
        var object = array[idx];
        var attrs = object["__modified"];
        if (attrs != undefined) {
            for (var attrIdx in attrs) {
                var attr = attrs[attrIdx];
                fn(object, attr, object[attr]);
            }
        }

        for (var attr in object) {
            var value = object[attr];
            if (typeof value === "object") {
                for (var idx in thisTurnModifiedItems) {
                    var item = thisTurnModifiedItems[idx];

                    if (value === item) {
                        markAttributeModified(object, attr);
                        fn(object, attr, value);
                        break;
                    }
                }
            }
        }
    }
}

function saveGame() {
    if (!gameRunning) return;
    if (awaitingCallback) return;
    if (runningWalkthrough) return;
    setTimeout(function () {
        var start = (new Date).getTime();
        saveGameInternal();
        var diff = (new Date).getTime() - start;
    }, 250);
}

function saveGameInternal() {
    if (!gameRunning) return;
    if (awaitingCallback) return;
    if (!localStorage) return;
    try {
        localStorageTransactionId = localStorage.getItem("transaction");
        if (localStorageTransactionId == undefined) {
            localStorageTransactionId = 1;
        }
        else {
            localStorageTransactionId = 3 - localStorageTransactionId;
        }

        localStorageSet("output", allOutput);
        localStorageSet("output2", $("#divOutput").html());
        if (commandLog != null) {
            localStorageSet("commandLog", commandLog.join(";"));
        }
        localStorageSet("nextObjectId", nextObjectId);

        // Save all object creations
        var createId = 0;
        for (var idx in createdObjects) {
            createId++;
            localStorageSet("create" + createId, createdObjects[idx]);
        }
        localStorageSet("numCreates", createId);

        // Save all object type additions
        var addTypeId = 0;
        for (var idx in addedTypes) {
            addTypeId++;
            localStorageSet("addtype" + addTypeId, addedTypes[idx]);
        }
        localStorageSet("numAddTypes", addTypeId);

        // Save all object attribute changes
        var changeId = 0;
        generateSaveLog(function (object, attribute, value) {
            var valueType = TypeOf(value);
            if (object.name == "player" && StartsWith(attribute, "currentcommand")) return;
            changeId++;
            var key = "change" + changeId;
            var storeValue = value;
            switch (valueType) {
                case "stringlist":
                    storeValue = value.length;
                    var count = 0;
                    for (var idx in value) {
                        localStorageSet(key + "_" + count, value[idx]);
                        count++;
                    }
                    break;
                case "objectlist":
                    storeValue = value.length;
                    var count = 0;
                    for (var idx in value) {
                        localStorageSet(key + "_" + count, value[idx]._js_name);
                        count++;
                    }
                    break;
                case "stringdictionary":
                case "scriptdictionary":
                    var count = 0;
                    for (var dictKey in value) {
                        localStorageSet(key + "_k" + count, dictKey);
                        localStorageSet(key + "_v" + count, value[dictKey]);
                        count++;
                    }
                    storeValue = count;
                    break;
                case "objectdictionary":
                    var count = 0;
                    for (var dictKey in value) {
                        localStorageSet(key + "_k" + count, dictKey);
                        localStorageSet(key + "_v" + count, value[dictKey]._js_name);
                        count++;
                    }
                    storeValue = count;
                    break;
                case "object":
                    storeValue = value._js_name;
                    break;
                case "null":
                    storeValue = "";
            }

            localStorageSet(key, object._js_name + "." + attribute + "=" + valueType + ":" + storeValue);
        });
        localStorageSet("numChanges", changeId);

        // Save all object destroys
        var destroyId = 0;
        for (var idx in destroyedObjects) {
            destroyId++;
            localStorageSet("destroy" + destroyId, destroyedObjects[idx]);
        }
        localStorageSet("numDestroys", destroyId);

        localStorage.setItem("transaction", localStorageTransactionId);
    }
    catch (err) {
        reportError("Failed to save game: " + err);
    }
}

function loadGame() {
    if (!localStorage) return false;

    localStorageTransactionId = localStorage.getItem("transaction");
    if (localStorageTransactionId == undefined) {
        return false;
    }
    try {
        nextObjectId = parseInt(localStorageGet("nextObjectId"));

        // Load object creations

        var commandLogList = localStorageGet("commandLog");
        if (commandLogList != null) {
            commandLog = commandLogList.split(";");
        }
        addToCommandLog("* loaded game");

        var createCount = localStorageGet("numCreates");
        for (var i = 1; i <= createCount; i++) {
            var data = localStorageGet("create" + i);
            var params = data.split(";");
            // format is name;defaultTypeObject.name;objectType
            switch (params[2]) {
                case "object":
                    var array = allObjects;
                    break;
                case "exit":
                    var array = allExits;
                    break;
                case "timer":
                    break;
                case "turnscript":
                    break;
                default:
                    throw "Unhandled create object type " + params[2];
            }
            if (params[2] == "timer") {
                createtimer(params[0]);
            }
            else if (params[2] == "turnscript") {
                createturnscript(params[0]);
            }
            else {
                createInternal(params[0], array, GetObject(params[1]), params[2]);
                // TODO: Add to objectsNameMap
            }
        }

        // Load object type additions

        var addTypeCount = localStorageGet("numAddTypes");
        for (var i = 1; i <= addTypeCount; i++) {
            var data = localStorageGet("addtype" + i);
            var params = data.split(";");
            // format is object;type
            addTypeToObject(window[params[0]], window[params[1]]);
        }

        // Load object attribute changes

        var changeCount = localStorageGet("numChanges");
        for (var i = 1; i <= changeCount; i++) {
            var data = localStorageGet("change" + i);
            var dotPos = data.indexOf(".");
            var eqPos = data.indexOf("=");
            var colonPos = data.indexOf(":");
            var objectName = data.substring(0, dotPos);
            var attrName = data.substring(dotPos + 1, eqPos);
            var type = data.substring(eqPos + 1, colonPos);
            var valueString = data.substring(colonPos + 1);

            var object = window[objectName];
            var value = valueString;

            switch (type) {
                case "script":
                    eval("_temp_assignfn=" + valueString);
                    value = _temp_assignfn;
                    break;
                case "stringlist":
                    var count = parseInt(valueString);
                    value = new Array();
                    for (var listIdx = 0; listIdx < count; listIdx++) {
                        value.push(localStorageGet("change" + i + "_" + listIdx));
                    }
                    break;
                case "objectlist":
                    var count = parseInt(valueString);
                    value = new Array();
                    for (var listIdx = 0; listIdx < count; listIdx++) {
                        value.push(window[localStorageGet("change" + i + "_" + listIdx)]);
                    }
                    break;
                case "stringdictionary":
                    var count = parseInt(valueString);
                    value = new Object();
                    for (var listIdx = 0; listIdx < count; listIdx++) {
                        var dictKey = localStorageGet("change" + i + "_k" + listIdx);
                        var dictVal = localStorageGet("change" + i + "_v" + listIdx);
                        value[dictKey] = dictVal;
                    }
                    break;
                case "objectdictionary":
                    var count = parseInt(valueString);
                    value = new Object();
                    for (var listIdx = 0; listIdx < count; listIdx++) {
                        var dictKey = localStorageGet("change" + i + "_k" + listIdx);
                        var dictVal = localStorageGet("change" + i + "_v" + listIdx);
                        value[dictKey] = window[dictVal];
                    }
                    break;
                case "scriptdictionary":
                    var count = parseInt(valueString);
                    value = new Object();
                    for (var listIdx = 0; listIdx < count; listIdx++) {
                        var dictKey = localStorageGet("change" + i + "_k" + listIdx);
                        var dictVal = localStorageGet("change" + i + "_v" + listIdx);
                        eval("_temp_assignfn=" + dictVal);
                        value[dictKey] = _temp_assignfn;
                    }
                    break;
                case "object":
                    value = window[valueString];
                    break;
                case "null":
                    value = null;
                    break;
                case "int":
                    value = parseInt(valueString);
                    break;
                case "double":
                    value = parseFloat(valueString);
                    break;
                case "boolean":
                    value = (valueString == "true");
            }

            set(object, attrName, value, false);
        }

        // Load object destroys

        var destroyCount = localStorageGet("numDestroys");
        for (var i = 1; i <= destroyCount; i++) {
            var data = localStorageGet("destroy" + i);
            destroy(data);
        }

        currentFont = GetObject("game").defaultfont;
        $("#fontOptions").val(currentFont);

        currentFontSize = GetObject("game").defaultfontsize.toString();
        $("#fontSize").val(currentFontSize);

        $("#fontSample").css("font-family", currentFont);
        $("#fontSample").css("font-size", currentFontSize + "pt");

        clearScreen();
        $("#divOutput").html(localStorageGet("output2"));
        msg(localStorageGet("output"));

        beginningOfCurrentTurnScrollPosition = $("#gameContent").height();
        scrollToEnd();

        updateLists();
        return true;
    }
    catch (err) {
        reportError("Failed to load game: " + err);
        return false;
    }
}

var localStorageTransactionId;
var lastRead;

function localStorageSet(key, value) {
    localStorage.setItem("c" + localStorageTransactionId + key, value);
}

function localStorageGet(key) {
    lastRead = key;
    return localStorage.getItem("c" + localStorageTransactionId + key);
}

var currentWalkthroughSteps;
var runningWalkthrough = false;
var stepCount;
var walkthroughMaxSteps;
var walkthroughFinishCode;
//%%DEBUG START
var walkthroughUndoTest = false;
var walkthroughUndoStage;
var walkthroughUndoSteps = 0;
//%%DEBUG END

function runWalkthrough(name, startStep, maxSteps, cheatCode) {
    //%%DEBUG START
    msg("Running walkthrough " + name);
    //%%DEBUG END
    stepCount = 0;
    //%%DEBUG START
    walkthroughUndoStage = 1;
    //%%DEBUG END
    walkthroughMaxSteps = maxSteps;
    walkthroughFinishCode = cheatCode;
    var walkthrough = getElement(name);
    if (walkthrough) {
        currentWalkthroughSteps = addWalkthroughSteps(walkthrough);
        currentWalkthroughSteps.splice(0, startStep);
        runningWalkthrough = true;
        runWalkthroughSteps();
    }
    else {
        msg("No walkthrough of that name");
    }
}

function addWalkthroughSteps(walkthrough) {
    var list = new Array();
    if (walkthrough.parent != null) {
        list = list.concat(addWalkthroughSteps(walkthrough.parent));
    }
    list = list.concat(walkthrough.steps);
    return list;
}

var postStep = null;

function runWalkthroughSteps() {
    //%%DEBUG START
    if (walkthroughUndoTest) {
        if (walkthroughUndoStage == 1) {
            if (walkthroughUndoSteps == stepCount) {
                walkthroughUndoStage++;
            }
        }
        if (walkthroughUndoStage == 2) {
            if (stepCount > 0) {
                stepCount--;
                sendCommandInternal("undo");
                setTimeout(function () {
                    runWalkthroughSteps();
                }, 100);
            }
            else {
                msg("Finished undoing walkthrough steps");
            }
            return;
        }
    }
    //%%DEBUG END

    if (currentWalkthroughSteps == null || currentWalkthroughSteps.length == 0 || (walkthroughMaxSteps > 0 && stepCount >= walkthroughMaxSteps)) {
        //%%DEBUG START
        msg("Finished running walkthrough");
        //%%DEBUG END
        runningWalkthrough = false;
        saveGame();
        return;
    }

    var step = currentWalkthroughSteps.splice(0, 1)[0];

    if (step == "label:" + walkthroughFinishCode) {
        runningWalkthrough = false;
        saveGame();
        return;
    }

    msg("");
    if (StartsWith(step, "assert:")) {
        //%%DEBUG START
        var expr = step.substring(7);
        msg("<b>Assert: </b>" + expr);
        if (eval(expr)) {
            msg("<span style=\"color:green\"><b>Pass</b></span>");
        }
        else {
            msg("<span style=\"color:red\"><b>Failed</b></span>");
            return;
        }
        //%%DEBUG END
    }
    else if (StartsWith(step, "label:")) {
        // ignore
    }
    else {
        stepCount++;
        //%%DEBUG START
        //msg("Step " + stepCount);
        console.log("*** WALKTHROUGH STEP " + stepCount + " ***");
        console.log("Command: " + step);
        //%%DEBUG END
        beginningOfCurrentTurnScrollPosition = $("#gameContent").height();
        //%%MAX V510
        msg("&gt; " + step);
        //%%END MAX V510
        sendCommandInternal(step);
        scrollToEndNow();
    }
    while (postStep) {
        var fn = postStep;
        postStep = null;
        fn();
    }

    setTimeout(function () {
        runWalkthroughSteps();
    }, 100);
}

function updateStatus(text) {
    if (text.length > 0) {
        showStatusVisible(true);
        $("#statusVars").html(text.replace(/\n/g, "<br/>"));
    }
    else {
        showStatusVisible(false);
    }
}

function setBackground(col) {
    $("#divOutput").css("background-color", col);
    $("#gamePanel").css("background-color", col);
}

function ASLEvent(event, parameter) {
    var fn = window[event];
    fn.apply(null, [parameter]);
}

function disableMainScrollbar() {
    $("#divOutput").css("overflow", "hidden");
}

function stopTimer() {
    clearInterval(tmrTick);
}

function getTickCountAndStopTimer() {
    stopTimer();
    return tickCount;
}

function goUrl(href) {
    window.open(href);
}

function setCompassDirections(directions) {
    _compassDirs = directions;
    $("#cmdCompassNW").attr("title", _compassDirs[0]);
    $("#cmdCompassN").attr("title", _compassDirs[1]);
    $("#cmdCompassNE").attr("title", _compassDirs[2]);
    $("#cmdCompassW").attr("title", _compassDirs[3]);
    $("#cmdCompassE").attr("title", _compassDirs[4]);
    $("#cmdCompassSW").attr("title", _compassDirs[5]);
    $("#cmdCompassS").attr("title", _compassDirs[6]);
    $("#cmdCompassSE").attr("title", _compassDirs[7]);
    $("#cmdCompassU").attr("title", _compassDirs[8]);
    $("#cmdCompassD").attr("title", _compassDirs[9]);
    $("#cmdCompassIn").attr("title", _compassDirs[10]);
    $("#cmdCompassOut").attr("title", _compassDirs[11]);
}

function setInterfaceString(name, text) {
    switch (name) {
        case "InventoryLabel":
            $("#inventoryLabel").html(text);
            break;
        case "PlacesObjectsLabel":
            $("#placesObjectsLabel").html(text);
            break;
        case "CompassLabel":
            $("#compassLabel").html(text);
            break;
        case "InButtonLabel":
            $("#cmdCompassIn").attr("value", text);
            break;
        case "OutButtonLabel":
            $("#cmdCompassOut").attr("value", text);
            break;
        case "EmptyListLabel":
            break;
        case "NothingSelectedLabel":
            break;
    }
}

function updateVerbButtons(list, verbsArray, idprefix) {
    var selectedIndex = list.prop("selectedIndex");
    var verbs = verbsArray[selectedIndex].split("/");
    var count = 1;
    $.each(verbs, function () {
        var target = $("#" + idprefix + count);
        target.attr("value", this);
        target.show();
        count++;
    });
    for (var i = count; i <= verbButtonCount; i++) {
        var target = $("#" + idprefix + i);
        target.hide();
    }
}
var _currentDiv = null;

function addText(text) {
    if (_currentDiv == null) {
        createNewDiv("left");
    }

    _currentDiv.append(text);
    scrollToEnd();
}

var _divCount = 0;

function createNewDiv(alignment) {
    _divCount++;
    $("<div/>", {
        id: "divOutputAlign" + _divCount,
        style: "text-align: " + alignment
    }).appendTo("#divOutput");
    _currentDiv = $("#divOutputAlign" + _divCount);
}

function bindMenu(linkid, verbs, text, inline) {
    var verbsList = verbs.split("/");

    var options = [];
    $.each(verbsList, function (key, value) {
        options = options.concat({ title: value, action: { type: "fn", callback: "doMenuClick('" + value.toLowerCase() + " " + text.replace("'", "\\'") + "');" } });
    });

    $("#" + linkid).jjmenu("both", options, {}, { show: "fadeIn", speed: 100, xposition: "left", yposition: "auto", "orientation": "auto" });
}

function doMenuClick(command) {
    $("div[id^=jjmenu]").remove();
    sendCommand(command);
}

function updateObjectLinks(data) {
    $(".elementmenu").each(function (index, e) {
        var $e = $(e);
        var verbs = data[$e.data("elementid")];
        if (verbs) {
            $e.removeClass("disabled");
            $e.data("verbs", verbs);
            // also set attribute so verbs are persisted to savegame
            $e.attr("data-verbs", verbs);
        } else {
            $e.addClass("disabled");
        }
    });
}

function updateExitLinks(data) {
    $(".exitlink").each(function (index, e) {
        var $e = $(e);
        var exitid = $e.data("elementid");
        var available = $.inArray(exitid, data) > -1;
        if (available) {
            $e.removeClass("disabled");
        } else {
            $e.addClass("disabled");
        }
    });
}

function updateCommandLinks(data) {
    $(".commandlink").each(function (index, e) {
        var $e = $(e);
        var exitid = $e.data("elementid");
        var available = $.inArray(exitid, data) > -1;
        if (available) {
            $e.removeClass("disabled");
        } else {
            $e.addClass("disabled");
        }
    });
}

function disableAllCommandLinks() {
    $(".commandlink").each(function (index, e) {
        $(e).addClass("disabled");
    });
}

function clearScreen() {
    allOutput = "";
    $("#divOutput").html("");
    createNewDiv("left");
    beginningOfCurrentTurnScrollPosition = 0;
}

function keyPressCode(e) {
    var keynum
    if (window.event) {
        keynum = e.keyCode
    } else if (e.which) {
        keynum = e.which
    }
    return keynum;
}

function AddYouTube(id) {
    var embedHTML = "<object width=\"425\" height=\"344\"><param name=\"movie\" value=\"http://www.youtube.com/v/" + id + "\"></param><param name=\"allowFullScreen\" value=\"true\"></param><param name=\"allowscriptaccess\" value=\"always\"></param><embed src=\"http://www.youtube.com/v/" + id + "\" type=\"application/x-shockwave-flash\" allowscriptaccess=\"always\" allowfullscreen=\"true\" width=\"425\" height=\"344\"></embed></object>";
    addText(embedHTML);
}

function AddVimeo(id) {
    var embedHTML = "<object width=\"400\" height=\"225\"><param name=\"allowfullscreen\" value=\"true\" /><param name=\"allowscriptaccess\" value=\"always\" /><param name=\"movie\" value=\"http://vimeo.com/moogaloop.swf?clip_id=" + id + "&amp;server=vimeo.com&amp;show_title=0&amp;show_byline=0&amp;show_portrait=0&amp;color=00adef&amp;fullscreen=1&amp;autoplay=0&amp;loop=0\" /><embed src=\"http://vimeo.com/moogaloop.swf?clip_id=" + id + "&amp;server=vimeo.com&amp;show_title=0&amp;show_byline=0&amp;show_portrait=0&amp;color=00adef&amp;fullscreen=1&amp;autoplay=0&amp;loop=0\" type=\"application/x-shockwave-flash\" allowfullscreen=\"true\" allowscriptaccess=\"always\" width=\"400\" height=\"225\"></embed></object>";
    addText(embedHTML);
}

function SetMenuBackground(color) {
    var css = getCSSRule("div.jj_menu_item");
    if (css) {
        css.style.backgroundColor = color;
    }
}

function SetMenuForeground(color) {
    var css = getCSSRule("div.jj_menu_item");
    if (css) {
        css.style.color = color;
    }
}

function SetMenuHoverBackground(color) {
    var css = getCSSRule("div.jj_menu_item_hover");
    if (css) {
        css.style.backgroundColor = color;
    }
}

function SetMenuHoverForeground(color) {
    var css = getCSSRule("div.jj_menu_item_hover");
    if (css) {
        css.style.color = color;
    }
}

function SetMenuFontName(font) {
    var css = getCSSRule("div.jjmenu");
    if (css) {
        css.style.fontFamily = font;
    }
}

function SetMenuFontSize(size) {
    // disabled
    //var css = getCSSRule("div.jjmenu");
    //if (css) {
    //    css.style.fontSize = size;
    //}
}

function TurnOffHyperlinksUnderline() {
    var css = getCSSRule("a.cmdlink");
    if (css) {
        css.style.textDecoration = "none";
    }
}

var _outputSections = new Array();

function JsStartOutputSection(name) {
    if ($.inArray(name, _outputSections) == -1) {
        _outputSections.push(name);
        createNewDiv("left");
    }
}

function JsEndOutputSection(name) {
    var index = $.inArray(name, _outputSections);
    if (index != -1) {
        _outputSections.splice(index, 1);
        createNewDiv("left");
    }
}

function JsHideOutputSection(name) {
    EndOutputSection(name);
    $("." + name + " a").attr("onclick", "");
    setTimeout(function () {
        $("." + name).hide(250, function () { $(this).remove(); });
    }, 250);
}

function getCSSRule(ruleName, deleteFlag) {
    ruleName = ruleName.toLowerCase();
    if (document.styleSheets) {
        for (var i = 0; i < document.styleSheets.length; i++) {
            var styleSheet = document.styleSheets[i];
            var ii = 0;
            var cssRule = false;
            do {
                if (styleSheet.cssRules) {
                    cssRule = styleSheet.cssRules[ii];
                } else if (styleSheet.rules) {
                    cssRule = styleSheet.rules[ii];
                }
                if (cssRule) {
                    if (typeof cssRule.selectorText != "undefined") {
                        if (cssRule.selectorText.toLowerCase() == ruleName) {
                            if (deleteFlag == 'delete') {
                                if (styleSheet.cssRules) {
                                    styleSheet.deleteRule(ii);
                                } else {
                                    styleSheet.removeRule(ii);
                                }
                                return true;
                            } else {
                                return cssRule;
                            }
                        }
                    }
                }
                ii++;
            } while (cssRule)
        }
    }
    return false;
}

function killCSSRule(ruleName) {
    return getCSSRule(ruleName, 'delete');
}

function addCSSRule(ruleName) {
    if (document.styleSheets) {
        if (!getCSSRule(ruleName)) {
            if (document.styleSheets[0].addRule) {
                document.styleSheets[0].addRule(ruleName, null, 0);
            } else {
                document.styleSheets[0].insertRule(ruleName + ' { }', 0);
            }
        }
    }
    return getCSSRule(ruleName);
}

function uiDoRestart() {
    if (localStorage) {
        localStorage.clear();
    }
    window.location.reload();
}

function reportError(errorMessage) {
    alert(errorMessage);
    console.log(errorMessage);
}

// WORLDMODEL ===================================================================================================================

var webPlayer = true;
var tmrTick = null;
var awaitingCallback = false;
var gameRunning = true;
var gameActive = true;

function worldmodelInitialise() {
    resolveObjectReferences();
    GetObject("game").timeelapsed = 0;
    for (var idx in allTimers) {
        var timer = allTimers[idx];
        if (timer.enabled) {
            timer.trigger = timer.interval;
        }
    }
    setObjectChildAttributes();
    if (typeof InitInterface == 'function') {
        InitInterface();
    }
    updateLists();
    tmrTick = setInterval(function () {
        timerTick();
    }, 1000);
}

function worldModelBeginGame() {
    StartGame();
    TryRunOnReadyScripts();
    updateLists();
}

function resolveObjectReferences() {
    for (var item in objectReferences) {
        var objData = objectReferences[item];
        window[objData[0]][objData[1]] = window[objData[2]];
    }
    for (var item in objectListReferences) {
        var objData = objectListReferences[item];
        var parent = window[objData[0]];
        var attribute = objData[1].replace(/ /g, "___SPACE___");
        var itemValue = objData[2];
        if (typeof parent[attribute] == "undefined") {
            parent[attribute] = new Array();
        }
        parent[attribute].push(window[itemValue]);
    }
    for (var item in objectDictionaryReferences) {
        var objData = objectDictionaryReferences[item];
        var parent = window[objData[0]];
        var attribute = objData[1].replace(/ /g, "___SPACE___");
        var itemKey = objData[2];
        var itemValue = objData[3];
        if (typeof parent[attribute] == "undefined") {
            parent[attribute] = new Object();
        }
        parent[attribute][itemKey] = window[itemValue];
    }
}

function setObjectChildAttributes() {
    for (var idx in allObjects) {
        var obj = allObjects[idx];
        if (obj.parent) {
            addChildObject(obj.parent, obj);
        }
    }
}

function addChildObject(parent, child) {
    if (!parent["_children"]) {
        parent["_children"] = new Array();
    }
    parent["_children"].push(child);
}

function updateLists() {
    setTimeout(function () {
        updateListsInternal();
    }, 1000);
}

function updateListsInternal() {
    updateObjectsLists();
    updateExitsList();
    if (typeof UpdateStatusAttributes == "function") {
        UpdateStatusAttributes();
    }
}

function updateObjectsLists() {
    updateObjectsList("GetPlacesObjectsList", "placesobjects");
    updateObjectsList("ScopeInventory", "inventory");
}

function updateObjectsList(scope, listName) {
    var listItems = window[scope]();
    if (scope == "GetPlacesObjectsList") {
        listItems = listItems.concat(ScopeExits());
    }
    var listData = new Array();
    for (var item in listItems) {
        var verbs = (listName == "inventory") ? listItems[item].inventoryverbs : listItems[item].displayverbs;
        if (verbs != undefined) {
            var verbsList = verbs.join("/");
        }
        else {
            var verbsList = "";
        }
        listData.push(GetDisplayAlias(listItems[item]) + ":" + verbsList);
    }
    updateList(listName, listData);
}

function updateExitsList() {
    var listItems = ScopeExits();
    var listData = new Array();
    for (var item in listItems) {
        listData.push(listItems[item].alias);
    }
    updateCompass(listData);
}

function attributeChanged(object, attribute, runscript) {
    // TO DO: "Meta" field SortIndex - changed when object moves to a new parent, so it appears at the end of the list
    // of children.
    markAttributeModified(object, attribute);
    if (runscript) {
        var changedScript = "changed" + attribute;
        if (typeof object[changedScript] == "function") {
            object[changedScript]();
        }
    }
}

var nextObjectId = 0;

function getUniqueId() {
    nextObjectId++;
    return "dynid" + nextObjectId;
}

var transactions = new Array();
var currentTransaction;

function preAttributeChange(object, attribute, newValue) {
    if (currentTransaction != undefined) {
        // store the old value on the undo list
        var oldValue = object[attribute];
        var undoFunction;
        if (attribute == "parent") {
            undoFunction = function () {
                newValue = object[attribute];
                object[attribute] = oldValue;
                objectMoved(object, newValue, oldValue);
            };
        }
        else {
            undoFunction = function () {
                object[attribute] = oldValue;
            };
        }

        currentTransaction.undolist.push(undoFunction);
    }

    var type = TypeOf(newValue);

    // if value requires cloning first then return a clone
    if (type == "stringdictionary" || type == "objectdictionary" || type == "scriptdictionary") {
        var result = new Object();
        for (key in newValue) {
            result[key] = newValue[key];
        }
        return result;
    }
    else if (type == "objectlist" || type == "stringlist") {
        var result = new Array();
        for (idx in newValue) {
            result.push(newValue[idx]);
        }
        return result;
    }

    return newValue;
}

function markAttributeModified(object, attribute) {
    if (object["__modified"] == undefined) {
        object["__modified"] = new Array();
    }
    if (object["__modified"].indexOf(attribute) == -1) {
        object["__modified"].push(attribute);
    }
}

var thisTurnModifiedItems = new Array();

function markModified(item) {
    if (thisTurnModifiedItems.indexOf(item) == -1) {
        thisTurnModifiedItems.push(item);
    }
}

// Javascript magic to support function overloading
// from http://ejohn.org/blog/javascript-method-overloading/
// addMethod - By John Resig (MIT Licensed)

function addMethod(object, name, fn) {
    var old = object[name];
    object[name] = function () {
        if (fn.length == arguments.length)
            return fn.apply(this, arguments);
        else if (typeof old == 'function')
            return old.apply(this, arguments);
    };
}

// Script commands

var objectTag = new XRegExp("\<object (id='(.*?)' )?verbs='(?<verbs>.*?)'\>(?<text>.*?)\<\/object\>");
var colorTag = /\<color color="(.*?)"\>(.*?)\<\/color\>/;
var commandTag = /\<command input="(.*?)"\>(.*?)\<\/command\>/;
var alignTag = /\<align align="(.*?)"\>(.*?)\<\/align\>/;
var fontTag = /\<font size="(.*?)"\>(.*?)\<\/font\>/;
var currentFont = "";
var currentFontSize = "";
var currentForeground = "";
var currentLinkForeground = "";
var nextID = 1;
var allOutput = "";

function msg(text) {
    //%%MIN V540
    OutputText(text);
    //%%END MIN V540

    //%%MAX V530
    if (allOutput.length > 0) allOutput += "<br/>";
    allOutput += text;
    var menuBindings = new Array();
    var cmdBindings = new Array();

    var count = 0;

    XRegExp.iterate(text, objectTag, function (matches, index, str) {
        count++;
    });

    var outputCount = 100 - count;

    while (objectTag.test(text)) {
        outputCount++;
        var matches = objectTag.exec(text);
        var style = "";
        if (currentLinkForeground.length > 0) {
            style = "style=\"color:" + currentLinkForeground + "\" ";
        }
        var linkToAdd;
        if (outputCount > 0) {
            var linkID = "verbLink" + nextID;
            linkToAdd = "<a id=\"" + linkID + "\" " + style + "class=\"cmdlink\">" + matches.text + "</a>";
            menuBindings.push([linkID, matches.verbs, matches.text]);
            nextID++;
        }
        else {
            linkToAdd = matches[3];
        }
        text = text.substring(0, matches.index) + linkToAdd + text.substring(matches.index + matches[0].length);
    }

    while (colorTag.test(text)) {
        var matches = colorTag.exec(text);
        var textToAdd = "<span style=\"color:" + matches[1] + "\">" + matches[2] + "</span>";
        text = text.substring(0, matches.index) + textToAdd + text.substring(matches.index + matches[0].length);
    }

    while (alignTag.test(text)) {
        var matches = alignTag.exec(text);
        var textToAdd = "<div style=\"text-align:" + matches[1] + "\">" + matches[2] + "</div>";
        text = text.substring(0, matches.index) + textToAdd + text.substring(matches.index + matches[0].length);
    }

    while (fontTag.test(text)) {
        var matches = fontTag.exec(text);
        var textToAdd = "<span style=\"font-size:" + matches[1] + "pt\">" + matches[2] + "</span>";
        text = text.substring(0, matches.index) + textToAdd + text.substring(matches.index + matches[0].length);
    }

    while (commandTag.test(text)) {
        var matches = commandTag.exec(text);
        var linkID = "cmdLink" + nextID;
        var style = "";
        if (currentLinkForeground.length > 0) {
            style = "style=\"color:" + currentLinkForeground + "\" ";
        }
        nextID++;
        var linkToAdd = "<a id=\"" + linkID + "\" " + style + "class=\"cmdlink\">" + matches[2] + "</a>";
        text = text.substring(0, matches.index) + linkToAdd + text.substring(matches.index + matches[0].length);
        (function(m) {
            cmdBindings.push([linkID, function() {
                sendCommand(m);
            }]);
        })(matches[1]);
    }

    var style = "";
    if (currentFont.length > 0) {
        style += "font-family:" + currentFont + ";";
    }
    if (currentFontSize.length > 0) {
        style += "font-size:" + currentFontSize + "pt;";
    }
    if (currentForeground.length > 0) {
        style += "color:" + currentForeground + ";";
    }
    if (style.length > 0) {
        text = "<span style=\"" + style + "\">" + text + "</span>";
    }

    addText(text + "<br/>");

    for (var menuBinding in menuBindings) {
        var thisBinding = menuBindings[menuBinding];
        bindMenu(thisBinding[0], thisBinding[1], thisBinding[2], true);
    }

    for (var cmdBinding in cmdBindings) {
        var thisBinding = cmdBindings[cmdBinding];
        $("#" + thisBinding[0]).click(thisBinding[1]);
    }
    //%%END MAX V530
}

function listadd(list, item) {
    if (currentTransaction != undefined) {
        var undoFunction = function () {
            list.splice(list.length - 1, 1);
        }
        currentTransaction.undolist.push(undoFunction);
    }
    list.push(item);
    markModified(list);
}

function listremove(list, item) {
    var index = list.indexOf(item);
    if (index != -1) {
        if (currentTransaction != undefined) {
            var undoFunction = function () {
                listadd(list, item);
            }
            currentTransaction.undolist.push(undoFunction);
        }

        list.splice(index, 1);
    }
    markModified(list);
}

function dictionaryadd(dictionary, key, item) {
    if (currentTransaction != undefined) {
        var oldValue = dictionary[key];
        if (oldValue != undefined) {
            var undoFunction = function () {
                dictionary[key] = oldValue;
            }
        }
        else {
            var undoFunction = function () {
                delete dictionary[key];
            }
        }
        currentTransaction.undolist.push(undoFunction);
    }
    dictionary[key] = item;
    markModified(dictionary);
}

function dictionaryremove(dictionary, key) {
    if (currentTransaction != undefined) {
        var oldValue = dictionary[key];
        var undoFunction = function () {
            dictionary[key] = oldValue;
        }
        currentTransaction.undolist.push(undoFunction);
    }
    delete dictionary[key];
    markModified(dictionary);
}

function request(requestType, data) {
    switch (requestType) {
        case "UpdateLocation":
            updateLocation(data);
            break;
        case "SetStatus":
            updateStatus(data);
            break;
        case "SetInterfaceString":
            var splitString = data.split("=");
            var element = splitString[0];
            var string = splitString[1];
            setInterfaceString(element, string);
            break;
        case "SetCompassDirections":
            setCompassDirections(data.split(";"));
            break;
        case "Show":
            uiShow(requestShowHide_GetElement(data));
            break;
        case "Hide":
            uiHide(requestShowHide_GetElement(data));
            break;
        case "Foreground":
            currentForeground = data;
            break;
        case "Background":
            setBackground(data);
            break;
        case "LinkForeground":
            currentLinkForeground = data;
            break;
        case "FontName":
            currentFont = data;
            break;
        case "FontSize":
            currentFontSize = data;
            break;
        case "ClearScreen":
            clearScreen();
            break;
        case "SetPanelContents":
            setPanelContents(data);
            break;
        case "Log":
            break;
        case "Speak":
            break;
        default:
            throw "Request not supported: " + requestType + "; " + data;
    }
}

function requestShowHide_GetElement(element) {
    switch (element) {
        case "Panes":
            return "#gamePanes";
        case "Location":
            return "#location";
        case "Command":
            return "#divCommand";
        default:
            return "";
    }
}

function setPanelHeight() {
    setTimeout(function () {
        var height = $("#gamePanel").height();
        if ($("#gamePanel").html() == "") {
            // workaround for IE weirdness where an empty div has height
            height = 0;
            $("#gamePanel").hide();
        }
        else {
            $("#gamePanel").show();
        }
        $("#gamePanelSpacer").height(height);
        scrollToEnd();
    }, 100);
}

function setPanelContents(html) {
    $("#gamePanel").html(html);
    setPanelHeight();
}

function starttransaction(command) {
    var previousTransaction = currentTransaction;
    currentTransaction = new Object();
    transactions.push(currentTransaction);
    currentTransaction.undolist = new Array();
    currentTransaction.previous = previousTransaction;
    currentTransaction.command = command;
}

function undo() {
    if (currentTransaction) {
        var transactionToUndo = currentTransaction;
        if (dynamicTemplates["UndoTurn"]) {
            msg(overloadedFunctions.DynamicTemplate("UndoTurn", transactionToUndo.command));
        }
        else {
            msg("Undo: " + transactionToUndo.command);
        }
        currentTransaction = undefined;
        transactionToUndo.undolist.reverse();
        for (idx in transactionToUndo.undolist) {
            var fn = transactionToUndo.undolist[idx];
            fn();
        }
        currentTransaction = transactionToUndo.previous;
    }
    else {
        if (templates["NothingToUndo"]) {
            msg(templates["NothingToUndo"]);
        }
        else {
            msg("Nothing to undo");
        }
    }
}

function runscriptattribute2(object, attribute) {
    var fn = GetAttribute(object, attribute);
    fn.call(object);
}

function runscriptattribute3(object, attribute, parameters) {
    var fn = GetAttribute(object, attribute);
    fn.call(object, parameters);
}

function invoke(script, parameters) {
    if (parameters) {
        script.apply(null, [parameters["result"]]);
    } else {
        script();
    }
}

function error(message) {
    throw message;
}

function set(object, attribute, value, runscript) {
    if (runscript === undefined) {
        runscript = true;
    }
    attribute = attribute.replace(/ /g, "___SPACE___");
    var changed = (object[attribute] != value);

    value = preAttributeChange(object, attribute, value);

    if (attribute == "parent") {
        var oldParent = object[attribute];
    }

    object[attribute] = value;

    if (changed) {
        if (attribute == "parent") {
            objectMoved(object, oldParent, value);
        }

        attributeChanged(object, attribute, runscript);
    }
}

function objectMoved(object, oldParent, newParent) {
    if (object.elementtype == "object" && object.type == "object") {
        if (oldParent) {
            var idx = oldParent["_children"].indexOf(object);
            if (idx == -1) {
                throw "Object wasn't in room!";
            }
            oldParent["_children"].splice(idx, 1);
        }
        if (newParent) {
            if (!newParent["_children"]) {
                newParent["_children"] = new Array();
            }
            newParent["_children"].push(object);
        }
    }
}

var menuOptions;
var menuCallback;
var finishTurnAfterSelection;

function showmenu_async(title, options, allowCancel, callback) {
    showmenu_async_internal(title, options, allowCancel, callback, true);
}

function showmenu_async_internal(title, options, allowCancel, callback, finishTurn) {
    menuOptions = options;
    menuCallback = callback;
    awaitingCallback = true;
    finishTurnAfterSelection = finishTurn;

    if (runningWalkthrough) {
        var step = currentWalkthroughSteps.splice(0, 1);
        var response = step[0];
        if (response.substring(0, 5) == "menu:") {
            var selection = response.substring(5);
            var selectionKey = "";
            for (var option in options) {
                msg(options[option]);
                if (options[option] == selection) {
                    selectionKey = option;
                }
            }
            if (selectionKey.length == 0) {
                msg("Error running walkthrough - menu response was not present in menu");
            }
            else {
                postStep = function () {
                    msg(" - " + selection);
                    SetMenuSelection(selectionKey);
                };
            }
        }
        else {
            msg("Error running walkthrough - expected menu response");
        }
    }
    else {
        showMenu(title, options, allowCancel);
    }
}

function ask(question, callback) {
    if (runningWalkthrough) {
        var step = currentWalkthroughSteps.splice(0, 1);
        var response = step[0];
        if (response.substring(0, 7) == "answer:") {
            awaitingCallback = true;
            postStep = function () {
                awaitingCallback = false;
                callback(response.substring(7) == "yes");
                TryFinishTurn();
            };
        }
        else {
            msg("Error running walkthrough - expected ask response");
        }
    }
    else {
        var result = confirm(question);
        callback(result);
        TryFinishTurn();
    }
}

var waitCallback;

function wait_async(callback) {
    waitCallback = callback;
    awaitingCallback = true;
    beginWait();
}

var getinputCallback;
var awaitingInputCallback = false;

function getinput_async(callback) {
    getinputCallback = callback;
    awaitingCallback = true;
    awaitingInputCallback = true;
}

function create(name) {
    createInternal(name, allObjects, GetObject("defaultobject"), "object");
}

function createexit(name, from, to) {
    var newExit = createInternal(getUniqueId(), allExits, GetObject("defaultexit"), "exit");
    set(newExit, "alias", name);
    set(newExit, "parent", from);
    set(newExit, "to", to);
    return newExit;
}

function createexit_withtype(name, from, to, type) {
    var newExit = createexit(name, from, to);
    if (type) {
        addTypeToObject(newExit, type);
    }
}

function createtimer(name) {
    createdObjects.push(name + ";;timer");

    if (currentTransaction != undefined) {
        var undoFunction = function () {
            destroy(name);
        }
        currentTransaction.undolist.push(undoFunction);
    }

    newObject = new Object();
    // TODO: Add to object map
    window["object_" + name] = newObject;
    allTimers.push(newObject);
    newObject.elementtype = "timer";
    newObject.name = name;
    newObject["_js_name"] = name;
    return newObject;
}

function createturnscript(name) {
    return createInternal(name, allTurnScripts, "defaultturnscript", "turnscript");
}

var createdObjects = new Array();

function createInternal(name, array, defaultTypeObject, objectType) {

    createdObjects.push(name + ";" + defaultTypeObject.name + ";" + objectType);

    if (currentTransaction != undefined) {
        var undoFunction = function () {
            destroy(name);
        }
        currentTransaction.undolist.push(undoFunction);
    }

    newObject = new Object();
    window[name] = newObject;
    objectsNameMap[name] = newObject;
    elementsNameMap[name] = newObject;
    array.push(newObject);
    newObject.elementtype = "object";
    newObject.name = name;
    newObject["_js_name"] = name;
    newObject.type = objectType;
    addTypeToObject_NoLog(newObject, defaultTypeObject);
    return newObject;
}

var addedTypes = new Array();

function addTypeToObject(object, type) {
    addedTypes.push(object.name + ";" + type.name);
    addTypeToObject_NoLog(object, type);
}

function addTypeToObject_NoLog(object, type) {
    if (type != undefined) {
        for (var attribute in type) {
            if (object[attribute] == undefined) {
                object[attribute] = type[attribute];
            }
        }
    }
}

var destroyedObjects = new Array();

function destroy(name) {
    destroyedObjects.push(name);
    destroyObject(GetObject(name));
}

function destroyObject(object) {
    var childObjects = new Array();
    for (var idx in allObjects) {
        var thisObject = allObjects[idx];
        if (thisObject.parent == object) {
            childObjects.push(thisObject);
        }
    }
    for (var childObject in childObjects) {
        destroyObject(childObjects[childObject]);
    }
    destroyObject_removeFromArray(object, allObjects);
    destroyObject_removeFromArray(object, allExits);
    destroyObject_removeFromArray(object, allCommands);
    destroyObject_removeFromArray(object, allTurnScripts);

    if (currentTransaction != undefined) {
        var undoFunction = function () {
            delete object["__destroyed"];
        }
        currentTransaction.undolist.push(undoFunction);
    }
    object["__destroyed"] = true;
}

function destroyObject_removeFromArray(object, array) {
    var removeIdx = $.inArray(object, array);
    if (removeIdx != -1) {
        if (currentTransaction != undefined) {
            var undoFunction = function () {
                array.push(object);
            }
            currentTransaction.undolist.push(undoFunction);
        }
        array.splice(removeIdx, 1);
    }
}

function insertHtml(filename) {
    addText(embeddedHtml[filename]);
}

function picture(filename) {
    msg("<img src=\"" + filename + "\" onload=\"scrollToEnd();\" /><br />");
}

function playsound(file, wait, loop) {
    // TO DO: support wav format
    playMp3(file, wait, loop);
}

function stopsound() {
    stopAudio();
}

function pauseEvent() {
    gameActive = false;
}

function resumeEvent() {
    gameActive = true;
}

function timerTick() {
    if (!gameRunning) return;
    if (!gameActive) return;
    var tickCount = GetObject("game").timeelapsed + 1;
    set(GetObject("game"), "timeelapsed", tickCount);
    var scriptRan = false;
    for (var idx in allTimers) {
        var timer = allTimers[idx];
        if (timer.enabled) {
            if (tickCount >= timer.trigger) {
                set(timer, "trigger", timer.trigger + timer.interval);
                timer.script();
                scriptRan = true;
            }
        }
    }
    if (scriptRan) {
        saveGame();
        updateLists();
    }
}

function finish() {
    gameRunning = false;
    if (localStorage) {
        localStorage.clear();
    }
    $("#divCommand").hide();
}

var onReadyCallback = null;

function on_ready(callback) {
    if (!awaitingCallback) {
        callback();
    }
    else {
        onReadyCallback = callback;
    }
}

function getElement(name) {
    return elementsNameMap[name];
}

// Functions

function NewObjectList() {
    return new Array();
}

function NewStringList() {
    return new Array();
}

function NewDictionary() {
    return new Object();
}

function NewObjectDictionary() {
    return new Object();
}

function NewStringDictionary() {
    return new Object();
}

function ToString(value) {
    return value.toString();
}

function ToInt(value) {
    return parseInt(value);
}

function ToDouble(value) {
    return parseFloat(value);
}

function Join(array, separator) {
    return array.join(separator);
}

function Split(input, delimiter) {
    return input.split(delimiter);
}

function Trim(input) {
    return $.trim(input);
}

function LengthOf(input) {
    if (input == null) return 0;
    return input.length;
}

function StartsWith(input, text) {
    return input.indexOf(text) == 0;
}

function LCase(text) {
    return text.toLowerCase();
}

function UCase(text) {
    return text.toUpperCase();
}

function CapFirst(text) {
    return text.substring(0, 1).toUpperCase() + text.substring(1);
}

function Left(text, count) {
    return text.substring(0, count);
}

function Right(text, count) {
    return text.substring(text.length - count - 1);
}

function Mid(text, start, count) {
    return text.substr(start - 1, count);
}

function Instr(p1, p2, p3) {
    var input, search;
    if (p3 === undefined) {
        input = p1;
        search = p2;
        return input.indexOf(search) + 1;
    } else {
        var start = p1;
        input = p2;
        search = p3;
        return input.indexOf(search, start - 1) + 1;
    }
}

function Replace(input, text, newText) {
    return input.split(text).join(newText);
}

var regexCache = new Object();

function getRegex(regexString, cacheID) {
    var result = regexCache[cacheID];
    if (result) {
        return result;
    }
    result = new XRegExp(regexString, "i");
    regexCache[cacheID] = result;
    return result;
}

function IsRegexMatch(regexString, input, cacheID) {
    var regex = getRegex(regexString, cacheID);
    return regex.test(input);
}

function GetMatchStrength(regexString, input, cacheID) {
    var regex = getRegex(regexString, cacheID);
    var lengthOfTextMatchedByGroups = 0;
    var matches = regex.exec(input);
    var namedGroups = GetRegexNamedGroups(matches);
    for (var groupIdx in namedGroups) {
        if (matches[namedGroups[groupIdx]] != undefined) {
            lengthOfTextMatchedByGroups += matches[namedGroups[groupIdx]].length;
        }
    }
    return input.length - lengthOfTextMatchedByGroups;
}

function Populate(regexString, input, cacheID) {
    var regex = getRegex(regexString, cacheID);
    var matches = regex.exec(input);
    var result = new Object();
    var namedGroups = GetRegexNamedGroups(matches);
    for (var groupIdx in namedGroups) {
        if (matches[namedGroups[groupIdx]] != undefined) {
            var varName = namedGroups[groupIdx];
            var mapIndex = varName.indexOf("_map_");
            if (mapIndex != -1) {
                varName = varName.substring(mapIndex + 5);
            }
            result[varName] = matches[namedGroups[groupIdx]];
        }
    }
    return result;
}

function GetRegexNamedGroups(matches) {
    var result = new Array();
    for (var prop in matches) {
        if (matches.hasOwnProperty(prop)) {
            if (StartsWith(prop, "object") || prop.indexOf("_map_object") != -1
             || StartsWith(prop, "text") || prop.indexOf("_map_text") != -1
             || StartsWith(prop, "exit") || prop.indexOf("_map_exit") != -1) {
                result.push(prop);
            }
        }
    }
    return result;
}

function GetAttribute(element, attribute) {
    attribute = attribute.replace(/ /g, "___SPACE___");
    return element[attribute];
}

function GetBoolean(element, attribute) {
    if (HasBoolean(element, attribute)) {
        return GetAttribute(element, attribute);
    }
    return false;
}

function GetInt(element, attribute) {
    if (HasInt(element, attribute)) {
        return GetAttribute(element, attribute);
    }
    return 0;
}

function GetObject(element) {
    result = objectsNameMap[element];
    if (result == undefined) return result;
    if (result["__destroyed"]) return null;
    return result;
}

function GetTimer(name) {
    return GetObject(name);
}

function GetString(element, attribute) {
    if (HasString(element, attribute)) {
        return GetAttribute(element, attribute);
    }
    return null;
}

function HasAttribute(element, attribute) {
    return (GetAttribute(element, attribute) != undefined);
}

function HasBoolean(element, attribute) {
    return (TypeOf(GetAttribute(element, attribute)) == "boolean");
}

function HasInt(element, attribute) {
    return (TypeOf(GetAttribute(element, attribute)) == "int");
}

function HasObject(element, attribute) {
    return (TypeOf(GetAttribute(element, attribute)) == "object");
}

function HasString(element, attribute) {
    return (TypeOf(GetAttribute(element, attribute)) == "string");
}

function HasScript(element, attribute) {
    return (TypeOf(GetAttribute(element, attribute)) == "script");
}

function HasDelegateImplementation(element, attribute) {
    return (TypeOf(GetAttribute(element, attribute)) == "script");
}

function GetAttributeNames(element, includeInheritedAttributes) {
    var result = [];
    for (var name in element) {
        result.push(name);
    }
    return result;
}

function AllObjects() {
    return allObjects;
}

function AllExits() {
    return allExits;
}

function AllCommands() {
    return allCommands;
}

function AllTurnScripts() {
    return allTurnScripts;
}

function TypeOf(value) {
    return overloadedFunctions.TypeOf(value);
}

function OverloadedFunctions() {
    addMethod(this, "TypeOf", function (value) {
        var type = typeof value;
        if (type == "function") return "script";
        if (type == "object") {
            if (value == null) return "null";
            if (Object.prototype.toString.call(value) === '[object Array]') {
                // could be an objectlist or stringlist
                var allObjects = true;
                var allStrings = true;

                for (var index in value) {
                    var item = value[index];
                    if (typeof item != "string") allStrings = false;
                    if (typeof item != "object") allObjects = false;
                    if (!allStrings && !allObjects) break;
                }

                if (allStrings) return "stringlist";
                if (allObjects) return "objectlist";
                return "unknown";
            }
            else {
                // could be an object, stringdictionary, objectdictionary or scriptdictionary
                var allObjects = true;
                var allStrings = true;
                var allScripts = true;

                for (var key in value) {
                    var item = value[key];
                    if (typeof item != "string") allStrings = false;
                    if (TypeOf(item) != "object") allObjects = false;
                    if (typeof item != "function") allScripts = false;
                    if (!allStrings && !allObjects && !allScripts) break;
                }

                if (allStrings) {
                    return "stringdictionary";
                }
                if (allObjects) {
                    return "objectdictionary";
                }
                if (allScripts) {
                    return "scriptdictionary";
                }
                return "object";
            }
        }
        if (type == "boolean") return "boolean";
        if (type == "string") return "string";
        if (type == "number") {
            // TO DO: Also need to handle double
            return "int";
        }
        if (type == "undefined") return "null";

        // TO DO: Also valid: Delegate name
    });

    addMethod(this, "TypeOf", function (object, attribute) {
        return TypeOf(GetAttribute(object, attribute));
    });

    addMethod(this, "DynamicTemplate", function (name, arg1) {
        params = new Object();
        params["object"] = arg1;
        params["exit"] = arg1;
        params["text"] = arg1;
        return dynamicTemplates[name](params);
    });

    addMethod(this, "DynamicTemplate", function (name, arg1, arg2) {
        params = new Object();
        params["object1"] = arg1;
        params["object2"] = arg2;
        return dynamicTemplates[name](params);
    });

    addMethod(this, "Eval", function (expression) {
        return eval(expression);
    });

    addMethod(this, "Eval", function (expression, params) {
        for (var varname in params) {
            var varvalue = params[varname];
            eval("var " + varname + "=varvalue");
        }
        return eval(expression);
    });
}

var overloadedFunctions = new OverloadedFunctions();

function DictionaryContains(dictionary, key) {
    return dictionary[key] != undefined;
}

function DictionaryItem(dictionary, key) {
    return dictionary[key];
}

function StringDictionaryItem(dictionary, key) {
    return dictionary[key];
}

function ScriptDictionaryItem(dictionary, key) {
    return dictionary[key];
}

function ObjectDictionaryItem(dictionary, key) {
    return dictionary[key];
}

function DictionaryCount(dictionary) {
    var count = 0;
    for (key in dictionary) {
        count++;
    }
    return count;
}

function ListCombine(list1, list2) {
    return list1.concat(list2);
}

function ListExclude(list, element) {
    var listCopy = list.slice(0);
    var index = listCopy.indexOf(element);
    if (index != -1) {
        listCopy.splice(index, 1);
    }
    return listCopy;
}

function ListContains(list, element) {
    return ($.inArray(element, list) != -1);
}

function ListCount(list) {
    return list.length;
}

function ListItem(list, index) {
    return list[index];
}

function StringListItem(list, index) {
    return list[index];
}

function ObjectListItem(list, index) {
    return list[index];
}

function Template(name) {
    return templates["t_" + name];
}

// TO DO: Need overloads to handle passing function parameters
function RunDelegateFunction(object, attribute) {
    return GetAttribute(object, attribute)();
}

function Contains(parent, child) {
    if (child.parent == null || child.parent == undefined) return false;
    if (child.parent == parent) return true;
    return Contains(parent, child.parent);
}

function ShowMenu() {
    throw "Synchronous ShowMenu function is not supported. Use showmenu_async function instead";
}

function SetMenuSelection(result) {
    if (Object.prototype.toString.call(menuOptions) === '[object Array]') {
        awaitingCallback = false;
        menuCallback(menuOptions[result]);
    }
    else {
        awaitingCallback = false;
        menuCallback(result);
    }
    if (finishTurnAfterSelection) {
        TryFinishTurn();
    }
}

function GetExitByName(parent, name) {
    for (var idx in allExits) {
        var obj = allExits[idx];
        if (obj.parent == parent && obj.alias == name) {
            return obj.name;
        }
    }
}

function GetExitByLink(parent, to) {
    for (var idx in allExits) {
        var obj = allExits[idx];
        if (obj.parent == parent && obj.to == to) {
            return obj.name;
        }
    }
}

function GetFileURL(file) {
    return file;
}

function Ask(question) {
    if (runningWalkthrough) {
        msg("<i>" + question + "</i>");
        var step = currentWalkthroughSteps.splice(0, 1);
        var response = step[0];
        if (response.substring(0, 7) == "answer:") {
            return (response.substring(7) == "yes");
        }
        else {
            msg("Error running walkthrough - expected menu response");
        }
    }
    else {
        return confirm(question);
    }
}

function GetUniqueElementName(prefix) {
    return prefix + getUniqueId();
}

function TryFinishTurn() {
    updateLists();
    TryRunOnReadyScripts();
    if (!awaitingCallback) {
        saveGame();
        if (typeof FinishTurn == "function") {
            FinishTurn();
        }
    }
}

function TryRunOnReadyScripts() {
    if (awaitingCallback) return;
    if (onReadyCallback != null) {
        var callback = onReadyCallback;
        onReadyCallback = null;
        callback();
    }
}

function GetDirectChildren(element) {
    if (!element["_children"]) {
        return new Array();
    }
    return element["_children"];
}

function GetAllChildObjects(element) {
    var result = new Array();
    var directChildren = GetDirectChildren(element);
    for (var idx in directChildren) {
        var obj = directChildren[idx];
        result.push(obj);
        result = result.concat(GetAllChildObjects(obj));
    }
    return result;
}

function IsGameRunning() {
    return gameRunning;
}

function IsDefined(variable) {
    return true;
}

function GetRandomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

function SafeXML(input) {
    return input.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
}

function GetUIOption() {
    return null;
}

function DoesInherit(obj, type) {
    return ListContains(obj._types, type);
}

var templates = new Object();
var dynamicTemplates = new Object();
var allObjects = new Array();
var allExits = new Array();
var allCommands = new Array();
var allTurnScripts = new Array();
var allTimers = new Array();
var objectReferences = new Array();
var objectListReferences = new Array();
var objectDictionaryReferences = new Array();
var embeddedHtml = new Object();
var objectsNameMap = new Object();
var elementsNameMap = new Object();

