<?php
if(!isset($_GET['sid']))
{
	header("Location: index.php");
	exit;
}

$song = new Song(intval($_GET['sid']));
if($song->id == 0)
{
	header("Location: index.php");
	exit;
}

$page = 1;
if(isset($_GET['p']))
	$page = intval($_GET['p']);
$stats = getBestStatsFromSong($song, 10, $page);
?>
<div class="title"><?php echo makeSafeHTML($song->name); ?></div>
<div class="block">
	<div class="blocktitle">Best statistics</div>
	<div class="blockcontent">
		<?php echoStatsTable($stats, true); ?>
	</div>
</div>