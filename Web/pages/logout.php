<?php
if(isValidSessionkey())
{
	unset($_SESSION['UID']);
	header("Location: index.php");
	exit;
}else{
	die("Hacking attempt blocked.");
}
?>