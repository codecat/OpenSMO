<?php
if(isLoggedIn() && isMod())
{
	if(isset($_POST['add']))
	{
		if(!isValidSessionkey())
			die("Hack attempt blocked.");
		
		$title = makeSafeSQL($_POST['title']);
		$content = makeSafeSQL($_POST['content']);
		
		sqlQuery("INSERT INTO \"news\" (\"Author\",\"Title\",\"Content\") VALUES(".$me->id.",'$title','$content')");
		
		header("Location: index.php");
		exit;
	}
	?>
	<div class="title">[Mod] Add News</div>
	<div class="block">
		<div class="blocktitle">Post</div>
		<div class="blockcontent">
			<form method="post" action="index.php?page=mod_news">
				<p>Title:<br /><input type="text" name="title" class="halfwidth" /></p>
				<p>Contents:<br /><textarea name="content"></textarea></p>
				<?php echoHiddenSessionkey(); ?>
				<input type="submit" name="add" value="Add" />
			</form>
		</div>
	</div>
	<?php
}else{
	header("Location: index.php");
	exit;
}
?>