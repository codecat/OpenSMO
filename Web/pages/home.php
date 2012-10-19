<div class="title">OpenSMO News</div>
<?php
$res = sqlQuery("SELECT * FROM \"news\" ORDER BY \"ID\" DESC LIMIT 0,5");
foreach($res as $row)
{
	$author = new User(intval($row['Author']));
	?>
	<div class="block">
		<div class="blocktitle"><?php echo makeSafeHTML($row['Title']); ?></div>
		<div class="blockcontent">
			<p class="newsposter">Posted by <?php echo $author->linkName(); ?></p>
			<?php echo nl2br($row['Content']); ?>
		</div>
	</div>
	<?php
}
?>