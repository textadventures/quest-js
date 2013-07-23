function GameModel() {
	var self = this;
	self.objects = ko.observableArray();

	self.objectCount = ko.computed(function(){
		return self.objects().length;
	});
}

function GameObject(name) {
	var self = this;
	self.name = name;
	self.attributes = ko.observableArray();
}

$(function(){
	var game = new GameModel();
	game.objects.push(new GameObject("book"));
	ko.applyBindings(game);

	$("#game-content").html("Initialised");
});