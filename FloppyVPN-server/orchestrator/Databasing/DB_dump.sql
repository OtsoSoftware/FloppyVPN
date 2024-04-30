-- MySQL dump 10.13  Distrib 8.0.19, for Win64 (x86_64)
--
-- Host: 109.71.240.149    Database: floppyvpn_db
-- ------------------------------------------------------
-- Server version	8.0.22-13

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
SET @MYSQLDUMP_TEMP_LOG_BIN = @@SESSION.SQL_LOG_BIN;
SET @@SESSION.SQL_LOG_BIN= 0;

--
-- GTID state at the beginning of the backup 
--

SET @@GLOBAL.GTID_PURGED=/*!80000 '+'*/ '50affa9b-dad1-11ee-8d98-525400123456:1-5426';

--
-- Table structure for table `accounts`
--

DROP TABLE IF EXISTS `accounts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `accounts` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `login` varchar(24) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `when_registered` datetime NOT NULL,
  `paid_till` datetime NOT NULL,
  `days_left` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `accounts_UN` (`login`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='It is what it is.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `aliases`
--

DROP TABLE IF EXISTS `aliases`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `aliases` (
  `alias` char(24) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `login` varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  UNIQUE KEY `payment_aliases_UN` (`alias`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='Used to anonymize payments shared with friends and etc by aliasing the login or another identity fingerprint.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `captchas`
--

DROP TABLE IF EXISTS `captchas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `captchas` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `image` varchar(100) COLLATE utf8mb4_general_ci NOT NULL,
  `solution` varchar(16) COLLATE utf8mb4_general_ci NOT NULL,
  `solved` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `currencies`
--

DROP TABLE IF EXISTS `currencies`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `currencies` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `enabled` tinyint(1) NOT NULL DEFAULT '0',
  `currency_code` varchar(8) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `currency_name` varchar(24) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `payment_service` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'What service to use to process payments of this currency',
  `month_cost` decimal(20,10) NOT NULL,
  `minimum_sum` decimal(20,10) NOT NULL COMMENT 'The minimal total sum that can be paid in this currency',
  `icon` varchar(100) COLLATE utf8mb4_general_ci NOT NULL DEFAULT '/imgs/currencies/default.png',
  PRIMARY KEY (`id`),
  UNIQUE KEY `currencies_UN` (`currency_code`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='Prices per month in each currency and their codes, names and payment services.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `device_types`
--

DROP TABLE IF EXISTS `device_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `device_types` (
  `device_type` int NOT NULL,
  `description` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`device_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `karmas`
--

DROP TABLE IF EXISTS `karmas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `karmas` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `hashed_ip_address` varchar(130) COLLATE utf8mb4_general_ci NOT NULL,
  `times_banned` tinyint unsigned NOT NULL DEFAULT '0' COMMENT 'How many times user has been banned in the past',
  `banned_till` datetime NOT NULL,
  `softbanned_till` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='Dictionary of IP addresses and their karmas';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `logs`
--

DROP TABLE IF EXISTS `logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `logs` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `date_time` datetime NOT NULL,
  `sender` varchar(128) COLLATE utf8mb4_general_ci NOT NULL,
  `message` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `p2p_addresses`
--

DROP TABLE IF EXISTS `p2p_addresses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `p2p_addresses` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `address` varchar(64) COLLATE utf8mb4_general_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='Collection of payment addresses to be used when initializing p2p payments';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `payments`
--

DROP TABLE IF EXISTS `payments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payments` (
  `id` char(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Unique and anonymous payment ID in hash, also used in payment URL',
  `login` varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Login of the account balance of which is being topped up',
  `network` varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT 'Currency network',
  `currency_code` varchar(8) COLLATE utf8mb4_general_ci NOT NULL,
  `external_payment_id` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Payment ID of an extertal payments service',
  `months_to_add` int NOT NULL,
  `amount_to_pay` decimal(20,10) NOT NULL,
  `address_to_pay_to` varchar(256) COLLATE utf8mb4_general_ci NOT NULL,
  `comment_to_attach` varchar(64) COLLATE utf8mb4_general_ci NOT NULL DEFAULT '',
  `when_created` datetime NOT NULL,
  `to_be_paid_till` datetime NOT NULL,
  `status` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Current payment status, mostly for informing a user',
  `is_paid` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Also indicates has user received his time or not',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='All payments ever existed';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `requests`
--

DROP TABLE IF EXISTS `requests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `requests` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `date_time` datetime NOT NULL,
  `hashed_ip_address` bigint unsigned NOT NULL,
  `successful` tinyint(1) NOT NULL,
  `request` varchar(128) COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT 'What resource was requested',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=656 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='The journal of all requests to the system. Mainly used for karma calculation';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `vpn_configs`
--

DROP TABLE IF EXISTS `vpn_configs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `vpn_configs` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `server` bigint unsigned NOT NULL COMMENT 'Server of this config',
  `config` varchar(700) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `account` bigint unsigned NOT NULL COMMENT 'Account this config belongs to',
  `device_type` int NOT NULL COMMENT 'For which device this config is',
  PRIMARY KEY (`id`),
  KEY `vpn_configs_device_types_FK` (`device_type`),
  KEY `vpn_configs_accounts_FK` (`account`),
  KEY `vpn_configs_vpn_servers_FK` (`server`),
  CONSTRAINT `vpn_configs_accounts_FK` FOREIGN KEY (`account`) REFERENCES `accounts` (`id`),
  CONSTRAINT `vpn_configs_device_types_FK` FOREIGN KEY (`device_type`) REFERENCES `device_types` (`device_type`),
  CONSTRAINT `vpn_configs_vpn_servers_FK` FOREIGN KEY (`server`) REFERENCES `vpn_servers` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `vpn_servers`
--

DROP TABLE IF EXISTS `vpn_servers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `vpn_servers` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `socket` varchar(96) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT 'IP/DNS+IP socket by which it is possible to communicate with the server',
  `country_code` char(2) COLLATE utf8mb4_general_ci NOT NULL COMMENT 'Where''s this server located?',
  `max_configs` tinyint unsigned NOT NULL,
  `ipv4_address` varchar(16) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `ipv6_address` varchar(64) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `when_added` date NOT NULL COMMENT 'When this vpn server was added to the system',
  `last_alive` datetime NOT NULL COMMENT 'Last successful alive check',
  PRIMARY KEY (`id`),
  UNIQUE KEY `vpn_servers_UN` (`socket`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping routines for database 'floppyvpn_db'
--
SET @@SESSION.SQL_LOG_BIN = @MYSQLDUMP_TEMP_LOG_BIN;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-04-30 17:20:07
