<?php
if(isLoggedIn() && isAdmin())
{
	if(isset($_POST['save']))
	{
		if(!isValidSessionkey())
			die("Hack attempt blocked.");
		
		foreach($_POST['value'] as $key => $value)
			setSetting($key, $value);
		
		header("Location: index.php?page=admin_settings");
		exit;
	}
	?>
	<div class="block">
		<div class="blocktitle">[Admin] Settings</div>
		<div class="blockcontent">
			<form method="post" action="index.php?page=admin_settings">
				<table>
					<tr>
						<th width="25%">Key</th>
						<th width="75%">Value</th>
					</tr>
					<?php
					$res = sqlQuery("SELECT * FROM \"settings\"");
					foreach($res as $row)
					{
						?>
						<tr>
							<td><?php echo makeSafeHTML($row['Key']); ?></td>
							<td><input class="fullwidth" type="text" name="value[<?php echo makeSafeHTML($row['Key']); ?>]" value="<?php echo makeSafeHTML($row['Value']); ?>" /></td>
						</tr>
						<?php
					}
					?>
				</table>
				<?php echoHiddenSessionkey(); ?>
				<input type="submit" name="save" value="Save" />
			</form>
		</div>
	</div>
	<?php
}else{
	header("Location: index.php");
	exit;
}
?>