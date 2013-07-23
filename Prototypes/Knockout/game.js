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

	self.set = function(attribute, value) {
		if (!self.hasOwnProperty(attribute)) {
			self.attributes.push(attribute);
			self[attribute] = ko.observable(value);
			console.log("Added new attribute: " + self.name + "." + attribute + "=" + value);
		}
		else {
			self[attribute](value);
			console.log("Updated attribute: " + self.name + "." + attribute + "=" + value);
		}
	}
}

$(function(){
	var game = new GameModel();
	var book = new GameObject("book");
	game.objects.push(book);
	book.set("look", "Initial look description");
	book.set("look", "Updated look description");
	book.set("take", function(){
		console.log ("Ran initial take function");
	});
	book.set("take", function(){
		console.log ("Ran updated take function");
	});
	ko.applyBindings(game);

	$("#game-content").html("Initialised. Book description is '" + book.look() + "'");
	book.take()();
});