<?php
if(!isset($_GET['rid']))
{
	header("Location: index.php");
	exit;
}
?>
<div class="title" id="titleDiv">Room Loading...</div>
<div class="block">
	<div class="blocktitle">Song</div>
	<div class="blockcontent" id="songDiv">
		...
	</div>
</div>
<div class="block">
	<div class="blocktitle">Players</div>
	<div class="blockcontent" id="playerDiv">
		...
	</div>
</div>
<div class="block">
	<div class="blocktitle">Chat</div>
	<div class="blockcontent">
		<div class="chat" id="chatDiv"></div>
    <p><input type="text" id="chatInput" style="width:100%;" /></p>
	</div>
</div>
<script>
	<?php
	$rid = $_GET['rid'];
	if(!preg_match("/^([a-zA-Z0-9]{5})$/", $rid)) $rid = "";
	?>
	var roomID = "<?php echo $rid; ?>";
</script>