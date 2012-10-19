<?php
if(isLoggedIn())
{
	header("Location: index.php");
	exit;
}

if(isset($_POST['register']))
{
	if(!isValidSessionkey())
		die("Hack attempt blocked.");
	
	$okay = true;
	
	if($_POST['p'] != $_POST['p2'])
	{
		echo makeBlock("Error", "Passwords are not the same.");
		$okay = false;
	}
	
	if($_POST['e'] != $_POST['e2'])
	{
		echo makeBlock("Error", "Email addresses are not the same.");
		$okay = false;
	}
	
	if(isValidEmail($_POST['e']))
	{
		echo makeBlock("Error", "Email address does not seem valid.");
		$okay = false;
	}
	
	if($okay)
	{
		$username = makeSafeSQL($_POST['u']);
		$password = strtoupper(md5($_POST['p']));
		$email = makeSafeSQL($_POST['e']);
		
		$existingCheck = sqlQuery("SELECT * FROM \"users\" WHERE \"Username\"='".$username."' OR \"Email\"='".$email."'");
		if(count($existingCheck) != 0)
		{
			echo makeBlock("Error", "Username or email address already in use.");
			$okay = false;
		}
		
		if($okay)
		{
			execQuery("INSERT INTO main.users (\"Username\",\"Password\",\"Email\",\"Rank\",\"XP\") VALUES('$username','$password','$email',0,0)");
			header("Location: index.php?page=login&r");
			exit;
		}
	}
}
?>
<div class="title">Register</div>
<div class="block">
	<div class="blocktitle">Login</div>
	<div class="blockcontent">
		<form method="post" action="index.php?page=register">
			<p>Username:<br /><input type="text" name="u" /></p>
			<p>Password:<br /><input type="password" name="p" /> <input type="password" name="p2" /></p>
			<p>Email:<br /><input type="text" name="e" /> <input type="text" name="e2" /></p>
			<?php echoHiddenSessionkey(); ?>
			<input type="submit" name="register" value="Register"/>
		</form>
	</div>
</div>