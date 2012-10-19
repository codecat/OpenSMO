<?php
if(!isset($_GET['uid']))
{
	header("Location: index.php");
	exit;
}

$user = new User(intval($_GET['uid']));
if($user->id == 0)
{
	header("Location: index.php");
	exit;
}

$page = 1;
if(isset($_GET['p']))
	$page = intval($_GET['p']);
$stats = getLatestStatsFrom($user, 10, $page);
?>
<div class="title"><?php echo makeSafeHTML($user->username); ?></div>
<div class="block">
	<div class="blocktitle">Profile</div>
	<div class="blockcontent">
		<table>
			<tr>
				<td width="25%">XP:</td>
				<td width="25%"><?php echo formatInt($user->xp); ?></td>
				
				<td width="25%"></td>
				<td width="25%"></td>
			</tr>
			<tr>
				<td>Level:</td>
				<td><?php echo $user->level(); ?> <a href="index.php?page=help_levels">(?)</a></td>
				
				<td></td>
				<td></td>
			</tr>
		</table>
	</div>
</div>
<div class="block">
	<div class="blocktitle">Last played songs</div>
	<div class="blockcontent">
		<?php echoStatsTable($stats); ?>
	</div>
</div>