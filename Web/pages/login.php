<?php
if(isLoggedIn())
{
	header("Location: index.php");
	exit;
}

if(isset($_POST['login']))
{
	if(!isValidSessionkey())
		die("Hack attempt blocked.");
	
	$username = makeSafeSQL($_POST['u']);
	$password = strtoupper(md5($_POST['p']));
	
	$loginCheck = sqlQuery("SELECT * FROM \"users\" WHERE \"Username\"=\"$username\" AND \"Password\"=\"$password\"");
	if(count($loginCheck) == 1)
	{
		$user = new User($loginCheck[0]);
		$_SESSION['UID'] = $user->id;
		
		header("Location: index.php?page=profile&uid=".$user->id);
		exit;
	}else{
		echo makeBlock("Error", "Wrong username or password.");
	}
}

if(isset($_GET['r']))
	echo makeBlock("Success!", "You are now registered on OpenSMO. Log in <a href=\"index.php?page=login\">here</a>.");
?>
<div class="title">Login</div>
<div class="block">
	<div class="blocktitle">Login</div>
	<div class="blockcontent">
		<form method="post" action="index.php?page=login">
			<p>Username:<br /><input type="text" name="u" /></p>
			<p>Password:<br /><input type="password" name="p" /></p>
			<?php echoHiddenSessionkey(); ?>
			<input type="submit" name="login" value="Login"/>
		</form>
	</div>
</div>