--- When the player interacts with the event by facing it and pressing Interact.
function Interact()
	if GetFlag("TestFlag") == false then
		Text("00001") -- It's locked...
		return coroutine.yield()
	else
		Text("00002") -- Boat dialog
	end
	Text("00003")
end

--- When the player walks on said event.
function WalkOn()

end