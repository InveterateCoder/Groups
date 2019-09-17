﻿var app;
document.addEventListener('DOMContentLoaded', () => {
    app = new app_class();
    window.addEventListener('resize', () => {
        app.on_resize();
    });
});
class api_class {
    constructor() { }
    async register(chatterer) {
        try {
            let ret = false;
            let resp = await this.post('/api/account/reg', chatterer);
            switch (resp) {
                case "pending_" + chatterer.email:
                    ret = true;
                    break;
                case "signed_out":
                    app.alert("You've been signed out, try again");
                    break;
                case "email_is_taken":
                    app.alert("This email address has been already registered");
                    break;
                case "name_is_taken":
                    app.alert("This nickname has been already taken");
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async validate(number) {
        try {
            let ret = false;
            let resp = await this.post('api/account/val', number);
            if (resp.startsWith('added_'))
                ret = true;
            else if (resp == 'signed_out')
                app.alert("You've been signed out, try again");
            else if (resp == 'invalid_confirmation_id')
                app.alert('Wrong number');
            else if (resp == 'reg_request_required')
                app.alert('Registration is required prior to email verification');
            else if (resp == 'email_is_taken')
                app.alert("This email address has been already registered");
            else if (resp == 'name_is_taken')
                app.alert("This nickname has been already taken");
            else app.alert(resp);
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async signin(chatterer) {
        try {
            let ret = false;
            let resp = await this.post('api/account/sign', chatterer);
            if (resp == "OK")
                ret = await this.usr_info();
            else if (resp == 'signed_out')
                app.alert("You've been signed out, try again");
            else if (resp == 'user_not_found')
                app.alert("The email address is not registered");
            else if (resp == 'password_incorrect')
                app.alert('Wrong password');
            else if (resp == 'multiple_signins_forbidden')
                app.alert('Access denied, already signed in');
            else
                app.alert(resp);
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async signout() {
        try {
            let ret = false;
            let resp = await this.post('api/account/user/signout');
            if (resp == 'OK' || resp == "not_authorized" || resp == "single_connection_only")
                ret = true;
            else
                app.alert(resp);
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async acc_change(request) {
        try {
            let ret = {
                name: false,
                pass: false
            };
            let resp = await this.post('api/account/user/change', request);
            switch (resp) {
                case "name_changed":
                    ret.name = true;
                    app.name = request.NewName;
                    break;
                case "pass_changed":
                    ret.pass = true;
                    break;
                case "name&pass_changed":
                    ret.name = true;
                    ret.pass = true;
                    app.name = request.NewName;
                    break;
                case "wrong_password":
                    app.alert("Wrong password")
                    break;
                case "name_exists":
                    app.alert("The name is already taken, choose another one");
                    break;
                case "no_change_requested":
                    app.alert("Empty request");
                    break;
                case "same_credentials":
                    app.alert("No changes have been made, same credentials");
                    break;
                case "not_authorized":
                case "single_connection_only":
                    ret.pass = true;
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async acc_delete(signin) {
        try {
            let ret = false;
            let resp = await this.post('api/account/user/del', signin);
            switch (resp) {
                case "deleted":
                    ret = true;
                    break;
                case "wrong_email":
                    app.alert("Wrong email address");
                    break;
                case "wrong_password":
                    app.alert("Wrong password");
                    break;
                case "not_authorized":
                case "single_connection_only":
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async grp_reg(info) {
        try {
            let ret = false;
            let resp = await this.post("api/groups/reg", info);
            switch (resp) {
                case "OK":
                    app.group = info.GroupName;
                    ret = true;
                    break;
                case "has_group":
                    app.alert("You have already registered a group");
                    break;
                case "name_taken":
                    app.alert("A group with such name already exists");
                    break;
                case "not_authorized":
                case "single_connection_only":
                    ret = true;
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async grp_change(info) {
        try {
            let ret = false;
            let resp = await this.post("api/groups/change", info);
            switch (resp) {
                case "name_changed":
                case "name&pass_changed":
                    app.group = info.NewGroupName;
                case "pass_changed":
                case "not_authorized":
                case "single_connection_only":
                    ret = true;
                    break;
                case "has_no_group":
                    app.group = null;
                    ret = true;
                    break;
                case "wrong_password":
                    app.alert("Wrong password");
                    break;
                case "group_name_exists":
                    app.alert("The group name is already taken, choose another one");
                    break;
                case "not_changed":
                    app.alert("No changes were made");
                    break;
                case "no_change_requested":
                    app.alert("No change was requested");
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async usr_info() {
        try {
            let ret = false;
            let fetresp = await fetch('api/account/user/info', {
                method: 'get',
                headers: {
                    'Accept': 'application/json'
                }
            });
            if (fetresp.status != 200) {
                app.name = null;
                app.group = null;
                app.ingroup = null;
                app.goto("reg");
            }
            else {
                let obj = await fetresp.json();
                if (obj.name != null) {
                    app.name = obj.name;
                    ret = true;
                }
                else
                    app.name = null;
                if (obj.group != null)
                    app.group = obj.group;
                else
                    app.group = null;
                if (obj.ingroup != null)
                    app.ingroup = obj.ingroup;
                else
                    app.ingroup = null;
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async grp_del(pass) {
        try {
            let ret = false;
            let resp = await this.post("api/groups/del", pass);
            switch (resp) {
                case "deleted":
                case "has_no_group":
                    app.group = null;
                    ret = true;
                    break;
                case "wrong_password":
                    app.alert("Wrong password");
                    break;
                case "not_authorized":
                case "single_connection_only":
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async list_groups(start, quantity, query) {
        let ret = null;
        if (start != 0 && !start || isNaN(start) || start < 0)
            return null;
        if (!quantity && quantity != 0)
            ret = await this.get(`api/groups/list/${start}`);
        else {
            if (isNaN(quantity) || quantity < 1 || quantity > 100)
                return null;
            if (!query && query != 0)
                ret = await this.get(`api/groups/list/${start}/${quantity}`);
            else {
                if (query.length > 64)
                    return null;
                else
                    ret = await this.get(`api/groups/list/${start}/${quantity}/${query}`);
            }
        }
        return ret;
    }
    async grp_signin(request) {
        let ret = {
            success: false,
            hide: false
        }
        try {
            let resp = await this.post("api/groups/sign", request);
            switch (resp) {
                case "OK":
                    app.ingroup = request.name;
                    ret.success = true;
                    break;
                case "already_signed":
                    ret.hide = true;
                    app.alert("Already signed into a group, please sign out to proceed");
                    break;
                case "not_found":
                    ret.hide = true;
                    app.alert("Group doesn't exist")
                    break;
                case "wrong_password":
                    app.alert("Wrong group password, try again")
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async grp_singout() {
        let ret = false;
        try {
            let resp = await this.post("api/groups/sign/out");
            switch (resp) {
                case "OK":
                case "not_signed":
                    app.ingroup = null;
                    ret = true;
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async get_members() {
        try {
            return await this.get("api/groups/members");
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async get_grp_msgs(ticks_str, quantity) {
        try {
            let ticks = Number(ticks_str);
            if (ticks != 0 && ticks < app.groupin.jsMsToTicks(Date.now() - 1296000000))
                return null;
            else {
                if (quantity < 1 || quantity > 100)
                    return null;
                else {
                    return await this.get(`api/groups/msgs/${ticks_str}/${quantity}`);
                }
            }
        }
        catch (err) {
            app.alert(err);
            return null;
        }
    }
    async get_missed_msgs(ticks_str) {
        try {
            let ticks = Number(ticks_str);
            if (ticks < app.groupin.jsMsToTicks(Date.now() - 86400000))
                return null;
            else return await this.get(`api/groups/msgs/missed/${ticks_str}`);
        }
        catch (err) {
            app.alert(err);
            return null;
        }
    }
    async post(addr, obj) {
        app.wait();
        let ret;
        try {
            let resp = await fetch(addr, {
                method: 'post',
                headers: {
                    'Accept': 'plain/text', 'Content-Type': 'application/json'
                },
                body: JSON.stringify(obj || '')
            });
            if (resp.status != 200) {
                document.cookie = "Auth_Tok=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
                app.name = null;
                app.group = null;
                app.goto('reg');
            }
            ret = await resp.text();
        }
        catch (err) {
            ret = err.message;
        }
        finally {
            app.resume();
            return ret;
        }
    }
    async get(addr) {
        let ret;
        try {
            let resp = await fetch(addr, {
                method: 'get',
                headers: {
                    'Accept': 'application/json'
                }
            });
            if (resp.status != 200) {
                document.cookie = "Auth_Tok=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
                app.name = null;
                app.group = null;
                app.goto('reg');
                ret = null;
            }
            else
                ret = await resp.json();
        }
        catch (err) {
            ret.response = err.message;
        }
        finally {
            return ret;
        }
    }
}

class reg_panel_class {
    constructor() {
        this.panel = document.getElementById('reg_panel');
        this.place = 'home';;
    }
    num(e) {
        if (isNaN(e.key))
            e.preventDefault();
    }
    pasteFilter(e) {
        if (isNaN(e.target.value))
            e.target.value = null;
    }
    clear() {
        let arr;
        switch (this.place) {
            case 'reg':
                arr = this.panel.children[2].getElementsByTagName('input');
                for (let i = 0; i < arr.length; i++)
                    arr[i].value = null;
                break;
            case 'sign':
                arr = this.panel.children[3].getElementsByTagName('input');
                for (let i = 0; i < arr.length; i++)
                    arr[i].value = null;
                break;
            case 'numb':
                this.panel.children[4].getElementsByTagName('input')[0].value = null;
                break;
        }
    }
    btn_hover(el) {
        el.firstElementChild.style.transform = "scale(1.1,1.1)";
    }
    btn_out(el) {
        el.firstElementChild.style.transform = "scale(1,1)";
    }
    btn_down(el) {
        el.firstElementChild.style.transform = "scale(0.9,0.9)";
    }
    btn_up(el) {
        el.firstElementChild.style.transform = "scale(1.1,1.1)";
    }
    goto(place) {
        switch (place) {
            case 'home':
                this.hide();
                this.panel.children[1].style.display = 'flex';
                this.place = place;
                break;
            case 'reg':
                this.hide();
                this.panel.children[2].style.display = 'flex';
                this.place = place;
                break;
            case 'sign':
                this.hide();
                this.panel.children[3].style.display = 'flex';
                this.place = place;
                break;
            case 'numb':
                this.hide();
                this.panel.children[4].style.display = 'flex';
                this.place = place;
                break;
        }
    }
    hide() {
        switch (this.place) {
            case 'home':
                this.panel.children[1].style.display = 'none';
                break;
            case 'reg':
                this.clear();
                this.panel.children[2].style.display = 'none';
                break;
            case 'sign':
                this.clear();
                this.panel.children[3].style.display = 'none';
                break;
            case 'numb':
                this.clear();
                this.panel.children[4].style.display = 'none';
                break;
        }
    }
    register() {
        let form = this.panel.children[2].getElementsByTagName('input');
        let chatterer = {
            name: form[0].value,
            email: form[1].value,
            password: form[2].value
        };
        if (!app.validateName(chatterer.name))
            app.alert(app.message.name("Name"));
        else if (!chatterer.email || !form[1].checkValidity())
            app.alert('Incorrect email address');
        else if (!app.validatePassword(chatterer.password))
            app.alert(app.message.password("Password"));
        else {
            app.api.register(chatterer).then(ok => {
                if (ok)
                    this.goto("numb");
            });
        }
    }
    validate() {
        let num = this.panel.children[4].getElementsByTagName('input')[0].value;
        if (num.length != 4)
            app.alert("Code must consist of 4 digits");
        else {
            app.api.validate(Number(num)).then(ok => {
                if (ok) {
                    this.goto("sign");
                }
            });
        }
    }
    signin() {
        let form = this.panel.children[3].getElementsByTagName('input');
        let chatterer = {
            email: form[0].value,
            password: form[1].value
        };
        if (!chatterer.email || !form[0].checkValidity())
            app.alert("Incorrect email address");
        else if (!app.validatePassword(chatterer.password))
            app.alert(app.message.password("Password"));
        else app.api.signin(chatterer).then(ok => {
            if (ok) {
                if (app.ingroup)
                    app.goto("ingroup");
                else app.goto('groups');
            }
        });
    }
}

class groups_class {
    constructor() {
        this.groups_window = document.getElementById('groups');
        this.hmbrgr_btn = document.getElementById('hmbrgr');
        this.m_acc = document.getElementById('m_account');
        this.m_group = document.getElementById('m_group');
        this.m_all = document.getElementById('m_all');
        this.account_btn = this.groups_window.children[2].children[2].children[0].children[0];
        this.group_btn = this.groups_window.children[2].children[2].children[0].children[1];
        this.acc_open = false;
        this.group_open = false;
        this.groups_forms = document.getElementById('groups_forms');
        this.start = 0;
        this.quantity = 100;
        this.query = '';
        this.timeoutHandle = null;
    }
    hmbrg_click() {
        this.hmbrgr_btn.classList.toggle('clicked');
        if (this.hmbrgr_btn.classList.contains('clicked')) {
            this.m_all.style.opacity = '1';
            this.m_all.style.transform = 'translate(0,0)';
        }
        else {
            this.m_all.style.opacity = '0';
            this.m_all.style.transform = `translate(0, -${this.m_all.offsetHeight}px)`;
        }
    }
    hmbrg_over() {
        for (let i = 0; i < this.hmbrgr_btn.children.length; i++) {
            this.hmbrgr_btn.children[i].style.backgroundColor = 'white';
            this.hmbrgr_btn.children[i].style.boxShadow = '0 0 4px white';
        }
    }
    hmbrg_out() {
        for (let i = 0; i < this.hmbrgr_btn.children.length; i++) {
            this.hmbrgr_btn.children[i].style.backgroundColor = 'silver';
            this.hmbrgr_btn.children[i].style.boxShadow = 'none';
        }
    }
    initialize(group = true) {
        this.close_all_menus();
        let height = this.groups_window.children[2].offsetHeight;
        this.groups_window.children[0].style.top = height + 'px';
        this.m_acc.style.right = '0px';
        this.m_acc.style.top = height + 'px';
        this.m_acc.style.width = this.account_btn.offsetWidth + 28 + 'px';
        this.m_group.style.right = this.m_acc.offsetWidth + 'px';
        this.m_group.style.top = height + 'px';
        this.m_group.style.width = this.group_btn.offsetWidth + 28 + 'px';
        if (group)
            this.group_conf();
        this.m_all.style.top = this.groups_window.children[2].offsetHeight + 'px';
        this.m_all.style.transform = `translate(0, -${this.m_all.offsetHeight}px)`;
    }
    group_conf() {
        if (app.group) {
            this.m_group.children[0].style.display = 'none';
            this.m_group.children[1].textContent = app.group;
            this.m_group.children[1].title = app.group;
            this.m_group.children[1].style.display = 'list-item';
            this.m_group.children[2].style.display = 'list-item';
            this.m_group.children[3].style.display = 'list-item';
            this.m_all.children[5].style.display = 'none';
            this.m_all.children[6].textContent = app.group;
            this.m_all.children[6].title = app.group;
            this.m_all.children[6].style.display = 'block';
            this.m_all.children[7].style.display = 'block';
            this.m_all.children[8].style.display = 'block';
        }
        else {
            this.m_group.children[0].style.display = 'list-item';
            this.m_group.children[1].style.display = 'none';
            this.m_group.children[2].style.display = 'none';
            this.m_group.children[3].style.display = 'none';
            this.m_all.children[5].style.display = 'block';
            this.m_all.children[6].style.display = 'none';
            this.m_all.children[7].style.display = 'none';
            this.m_all.children[8].style.display = 'none';
        }
    }
    m_accf() {
        this.m_proc(this.m_acc, this.account_btn, this.acc_open, true);
    }
    m_groupf() {
        this.m_proc(this.m_group, this.group_btn, this.group_open);
    }
    m_proc(el, btn, open, isacc = false) {
        if (open) {
            btn.classList.remove('open');
            if (isacc)
                this.acc_open = false;
            else
                this.group_open = false;
            el.style.opacity = '0';
            el.style.visibility = 'hidden';
        }
        else {
            btn.classList.add('open');
            el.style.visibility = 'visible';
            el.style.opacity = '1';
            el.focus();
            if (isacc)
                this.acc_open = true;
            else
                this.group_open = true;
        }
    }
    m_onblur(el) {
        el.style.opacity = '0';
        if (el.id == 'm_account')
            this.account_btn.classList.remove('open');
        else
            this.group_btn.classList.remove('open');
        setTimeout(() => {
            if (el.id == 'm_account') {
                this.acc_open = false;
                this.m_acc.style.visibility = 'hidden';
            }
            else {
                this.group_open = false;
                this.m_group.style.visibility = 'hidden';
            }
        }, 170);
    }
    close_all_menus() {
        if (this.acc_open) {
            this.account_btn.classList.remove('open');
            this.acc_open = false;
            this.m_acc.style.opacity = '0';
            this.m_acc.style.visibility = 'hidden';
        }
        else if (this.group_open) {
            this.group_btn.classList.remove('open');
            this.group_open = false;
            this.m_group.style.opacity = '0';
            this.m_group.style.visibility = 'hidden';
        }
        if (this.hmbrgr_btn.classList.contains('clicked')) {
            this.hmbrgr_btn.classList.remove('clicked');
            this.m_all.style.opacity = '0';
            this.m_all.style.transform = `translate(0, -${this.m_all.offsetHeight}px)`;
        }
    }
    groups_resize() {
        this.initialize(false);
        if (this.groups_window.firstElementChild.scrollHeight - this.groups_window.firstElementChild.scrollTop <  window.innerHeight + 300)
            this.list_load();
    }
    signout() {
        this.close_all_menus();
        app.api.signout().then(ok => {
            if (ok) {
                app.goto('reg');
                app.name = null;
                app.group = null;
            }
        });
    }
    form_open(n) {
        this.groups_forms.children[n].style.display = 'flex';
        this.groups_forms.style.display = 'block';
        this.close_all_menus();
    }
    form_close(n) {
        this.groups_forms.style.display = 'none';
        this.groups_forms.children[n].style.display = 'none';
        let form = this.groups_forms.children[n].getElementsByTagName('input');
        if (n == 4) {
            this.groups_forms.children[4].children[4].style.visibility = 'visible';
            form[2].checked = false;
            for (let i = 0; i < form.length; i++) {
                if (i == 2) continue;
                form[i].value = null;
            };
        }
        else
            for (let i = 0; i < form.length; i++) {
                form[i].value = null;
            };
    }
    acc_change() {
        let form = this.groups_forms.children[1].getElementsByTagName('input');
        let request = {
            Password: form[0].value,
            NewName: form[1].value,
            NewPassword: form[2].value
        };
        if (!app.validatePassword(request.Password))
            app.alert(app.message.password("Password"));
        else if (request.NewName.length > 0 && !app.validateName(request.NewName))
            app.alert(app.message.name("New name"));
        else if (request.NewPassword && !app.validatePassword(request.NewPassword))
            app.alert(app.message.password("New password"));
        else if (request.Password == request.NewPassword || request.NewName == app.name)
            app.alert("Leave blank if the credentials are the same");
        else if (!request.NewName && !request.NewPassword)
            app.alert("No change requested");
        else {
            if (request.NewName.length == 0)
                request.NewName = null;
            if (request.NewPassword.length == 0)
                request.NewPassword = null;
            app.api.acc_change(request).then(ret => {
                if (ret.name || ret.pass) {
                    if (ret.name)
                        this.groups_window.getElementsByTagName('code')[0].textContent = app.name;
                    this.form_close(1);
                }
            });
        }
    }
    acc_delete() {
        let form = this.groups_forms.children[2].getElementsByTagName('input');
        let signin = {
            email: form[0].value,
            password: form[1].value
        };
        if (!signin.email || !form[0].checkValidity())
            app.alert("Incorrect email address");
        else if (!app.validatePassword(signin.password))
            app.alert(app.message.password("Password"));
        else {
            app.api.acc_delete(signin).then(ret => {
                if (ret) {
                    app.goto('reg');
                    app.name = null;
                    app.group = null;
                    this.form_close(2);
                }
            });
        }
    }
    group_create() {
        let form = this.groups_forms.children[3].getElementsByTagName('input');
        let info = {
            Password: form[0].value,
            GroupName: form[1].value,
            GroupPassword: form[2].value
        };
        if (!app.validatePassword(info.Password))
            app.alert(app.message.password("Password"));
        else if (!app.validateName(info.GroupName))
            app.alert(app.message.name("Group name"));
        else if (info.GroupPassword && !app.validatePassword(info.GroupPassword))
            app.alert(app.message.password("Group password"));
        else {
            if (info.GroupPassword.length == 0)
                info.GroupPassword = null;
            app.api.grp_reg(info).then(ret => {
                if (ret) {
                    this.group_conf();
                    this.list_clear();
                    this.form_close(3);
                    this.list_load();
                }
            });
        }
    }
    group_change() {
        let form = this.groups_forms.children[4].getElementsByTagName('input');
        let info = {
            Password: form[0].value,
            NewGroupName: form[1].value,
            NewGroupPassword: form[3].value
        };
        if (!app.validatePassword(info.Password))
            app.alert(app.message.password("Password"));
        else if (info.NewGroupName.length > 0 && !app.validateName(info.NewGroupName))
            app.alert(app.message.name("New name"));
        else if (!form[2].checked && info.NewGroupPassword && !app.validatePassword(info.NewGroupPassword))
            app.alert(app.message.password("New password"));
        else {
            if (form[2].checked) info.NewGroupPassword = null;
            else if (info.NewGroupPassword.length == 0) info.NewGroupPassword = "000";
            if (info.NewGroupName.length == 0)
                info.NewGroupName = null; 
            if (!info.NewGroupName && info.NewGroupPassword == "000")
                app.alert("No change requested");
            else {
                app.api.grp_change(info).then(ret => {
                    if (ret) {
                        this.group_conf();
                        this.list_clear();
                        this.form_close(4);
                        this.list_load();
                    }
                });
            }
        }

    }
    group_del() {
        let pass = this.groups_forms.children[5].getElementsByTagName('input')[0].value;
        if (!app.validatePassword(pass))
            app.alert(app.message.password("Password"));
        else app.api.grp_del(pass).then(ret => {
            if (ret) {
                this.group_conf();
                this.list_clear();
                this.form_close(5);
                this.list_load();
            }
        });
    }
    on_pass_del_change(el) {
        if (el.checked)
            this.groups_forms.children[4].children[4].style.visibility = 'hidden';
        else
            this.groups_forms.children[4].children[4].style.visibility = 'visible';
    }
    list_clear() {
        let div = this.groups_window.firstElementChild;
        if (div.childElementCount > 0)
            while (div.firstElementChild)
                div.firstElementChild.remove();
        this.start = 0;
        this.quantity = 100;
    }
    list_add(list) {
        if (!list || !Array.isArray(list) || list.length == 0)
            return false;
        let div = this.groups_window.firstElementChild;
        list.forEach((str) => {
            if (typeof str == "string") {
                let node = document.createElement("button");
                var textnode = document.createTextNode(str);
                node.appendChild(textnode);
                node.onclick = () => app.groups.group_signin(str);
                node.tabIndex = -1;
                div.appendChild(node);
            }
        });
    }
    list_load() {
        if (this.quantity > 0) {
            app.api.list_groups(this.start, this.quantity, this.query).then(ret => {
                if (ret == 0)
                    this.quantity = 0;
                else {
                    if (typeof ret == "string")
                        app.alert(ret);
                    else if (Array.isArray(ret)) {
                        this.start += ret.length;
                        if (ret.length < this.quantity)
                            this.quantity = 0;
                        this.list_add(ret);
                        if (this.groups_window.firstElementChild.scrollHeight < window.innerHeight + 300)
                            this.list_load();
                    }
                }
            });
        }
    }
    on_scroll(el) {
        if (el.scrollTop + el.clientHeight > el.scrollHeight - 300) {
            this.list_load();
        }
    }
    on_query_key(query) {
        this.set_list_timer(query);
    }
    on_query_paste(e) {
        this.set_list_timer(e.clipboardData.getData('Text'));
    }
    on_query_cut(el) {
        setTimeout(() => {
            this.set_list_timer(el.value);
        }, 50);
    }
    set_list_timer(query) {
        if (query != this.query) {
            this.query = query;
            clearTimeout(this.timeoutHandle);
            this.timeoutHandle = setTimeout(() => {
                this.list_clear();
                this.list_load();
            }, 800);
        }
    }
    group_signin(group) {
        if (group && group == app.group) {
            this.close_all_menus();
            this.group_signin_send({ name: group, password: null });
        }
        else {
            let form = this.groups_forms.children[6].getElementsByTagName('input');
            form[0].value = group;
            this.form_open(6);
        }
    }
    group_signin_send(request = null) {
        if (!request) {
            let form = this.groups_forms.children[6].getElementsByTagName('input');
            request = {
                name: form[0].value,
                password: form[1].value
            }
        }
        if (!app.validateName(request.name))
            app.alert(app.message.name("Group name"));
        else if (request.password && !app.validatePassword(request.password))
            app.alert(app.message.password("Group password"));
        else {
            if (!request.password)
                request.password = null;
            app.api.grp_signin(request).then(ret => {
                if (ret.success) {
                    if (this.groups_forms.children[6].style.display != 'none')
                        this.form_close(6);
                    app.goto('ingroup');
                }
                else if (ret.hide) {
                    this.list_clear();
                    this.list_load();
                    if (this.groups_forms.children[6].style.display != 'none')
                        this.form_close(6);
                }
            });
        }
    }
}

class ingroup_class {
    constructor() {
        this.dicon_cover = document.querySelector("#ingroup > #disconnected");
        this.msgs_panel = document.getElementById("ingroup").children[0];
        this.msgs_panel.addEventListener("click", () => app.groupin.usrs_close(), true);
        this.msgs_cont = this.msgs_panel.children[1];
        this.usrs_panel = this.msgs_panel.nextElementSibling;
        this.open_btn = this.msgs_panel.firstElementChild.firstElementChild;
        this.close_btn = this.usrs_panel.firstElementChild.children[1];
        this.connection = new signalR.HubConnectionBuilder().withUrl("/hub").configureLogging(signalR.LogLevel.Error).build();
        this.connection.onclose(() => this.leave());
        this.connection.on("signed_out", name => this.member_signout(name));
        this.connection.on("go_off", name => this.usr_switch_off(name));
        this.connection.on("go_on", name => this.usr_joined(name));
        this.connection.on("message_client", msg => this.recieve_msg(msg));
        this.isMobile = false;
        this.usrs_panel_open = true;
        this.offl_usr = null;
        this.onl_usrs = new Set();
        this.btn_swtch = this.usrs_panel.children[2].firstElementChild;
        this.btn_notif = this.btn_swtch.nextElementSibling;
        this.is_cleared = false;
        this.onl_usr_panel = this.usrs_panel.children[3].firstElementChild;
        this.ofl_usr_panel = this.usrs_panel.children[3].children[2];
        this.arr_onl_usrs = null;
        this.arr_ofl_usrs = null;
        this.signingout = false;
        this.open_peers = null;
        this.upper_msg_el = null;
        this.upper_msg_time = "0";
        this.last_msg_time = null;
        this.end_reached = false;
        this.msgs_quantity = 20;
        this.isdown = false;
        this.scroll_handle = null;
    }
    init() {
        this.dicon_cover.style.display = "none";
        this.config_mobile();
        app.api.get_members().then(ret => {
            if (!Array.isArray(ret.online) || !Array.isArray(ret.offline)) {
                this.signout();
                app.alert("Something went terribly wrong: " + ret);
            }
            else {
                this.usrs_panel.children[1].textContent = app.name;
                this.arr_onl_usrs = ret.online;
                this.arr_ofl_usrs = ret.offline;
                this.arr_onl_usrs.splice(this.arr_onl_usrs.indexOf(app.name), 1);
                this.arr_onl_usrs.sort();
                this.arr_onl_usrs.forEach(name => this.onl_usr_panel.appendChild(this.create_name(name, true)));
                this.arr_ofl_usrs.sort();
                this.arr_ofl_usrs.forEach(name => {
                    this.ofl_usr_panel.appendChild(this.create_name(name, false));
                });
            }
        });
        if (!this.last_msg_time)
            this.get_msgs(true);
        else this.get_missed_msgs(this.last_msg_time);
    }
    leave() {
        this.dicon_cover.style.display = "block";
        this.offl_usr = null;
        this.onl_usrs.clear();
        this.is_cleared = false;
        while (this.onl_usr_panel.firstElementChild)
            this.onl_usr_panel.firstElementChild.remove();
        while (this.ofl_usr_panel.firstElementChild)
            this.ofl_usr_panel.firstElementChild.remove();
        this.btn_swtch.firstElementChild.src = "/images/cancel_sel.svg";
        this.btn_swtch.classList.add("disabled");
        this.btn_notif.classList.add("disabled");
    }
    ingroup_resize() {
        this.config_mobile();
        this.usrs_close();
        if (this.isdown)
            this.msgs_cont.scrollTop = this.msgs_cont.scrollHeight;
    }
    config_mobile() {
        if (this.isMobile && window.innerWidth > 900) {
            this.isMobile = false;
            this.usrs_open();
            this.open_btn.style.display = this.close_btn.style.display = "none"
            this.open_btn.style.opacity = this.close_btn.style.opacity = "0";
        }
        else if (!this.isMobile && window.innerWidth <= 900) {
            this.isMobile = true;
            this.usrs_close();
            this.open_btn.style.display = this.close_btn.style.display = "block"
            this.open_btn.style.opacity = "1";
            setTimeout(() => this.close_btn.style.opacity = "1", 400);
        }
    }
    usrs_close() {
        if (this.isMobile && this.usrs_panel_open) {
            this.usrs_panel_open = false;
            this.usrs_panel.style.transform = `translateX(-${this.usrs_panel.offsetWidth || 300}px)`;
        }
    }
    usrs_open() {
        if (!this.usrs_panel_open) {
            this.usrs_panel_open = true;
            this.usrs_panel.style.transform = `translateX(0)`;
        }
    }
    signout() {
        app.api.grp_singout().then(ret => {
            if (ret) {
                this.connection.invoke("SignOut").then(() => app.goto('groups')).catch(err => {
                    app.goto('groups');
                    app.alert(err.message);
                });
            }
        });
    }
    offl_usr_select(el) {
        if (this.offl_usr == null) {
            this.offl_usr = el;
            el.classList.add("selected");
            this.btn_notif.classList.remove("disabled");
        }
        else {
            if (this.offl_usr == el) {
                this.offl_usr = null;
                el.classList.remove("selected");
                this.btn_notif.classList.add("disabled");
            }
            else {
                this.offl_usr.classList.remove("selected");
                this.offl_usr = el;
                el.classList.add("selected");
            }
        }
    }
    onl_usr_select(el) {
        if (this.is_cleared) {
            this.onl_usrs.clear();
            this.btn_swtch.firstElementChild.src = "/images/cancel_sel.svg";
            this.onl_usrs.add(el);
            el.classList.add("selected");
            this.is_cleared = false;
        }
        else if (this.onl_usrs.has(el)) {
            this.onl_usrs.delete(el);
            el.classList.remove("selected");
            if (this.onl_usrs.size == 0) {
                this.btn_swtch.classList.add("disabled");
            }
        }
        else {
            if (this.onl_usrs.size == 0) {
                this.btn_swtch.classList.remove("disabled");
            }
            this.onl_usrs.add(el);
            el.classList.add("selected");
        }
    }
    switch_click() {
        if (this.onl_usrs.size != 0) {
            if (this.is_cleared) {
                this.onl_usrs.forEach(el => el.classList.add("selected"));
                this.is_cleared = false;
                this.btn_swtch.firstElementChild.src = "/images/cancel_sel.svg";
            }
            else {
                this.onl_usrs.forEach(el => el.classList.remove("selected"));
                this.is_cleared = true;
                this.btn_swtch.firstElementChild.src = "/images/selective.svg";
            }
        }
    }
    create_name(name, online) {
        let div = document.createElement("div");
        div.textContent = name;
        div.setAttribute("onclick", online ? "app.groupin.onl_usr_select(this)" : "app.groupin.offl_usr_select(this)");
        return div;
    }
    usr_joined(member) {
        let index = this.arr_onl_usrs.indexOf(member);
        if (index > -1)
            return;
        index = this.arr_ofl_usrs.indexOf(member);
        if (index > -1) {
            if (this.offl_usr && member == this.offl_usr.textContent) {
                this.offl_usr = null;
                this.btn_notif.classList.add("disabled");
            }
            this.arr_ofl_usrs.splice(index, 1);
            this.ofl_usr_panel.children[index].remove();
        }
        this.arr_onl_usrs.push(member);
        this.arr_onl_usrs.sort(function (a, b) {
            if (a.toLowerCase() < b.toLowerCase()) return -1;
            if (a.toLowerCase() > b.toLowerCase()) return 1;
            return 0;
        });
        index = this.arr_onl_usrs.indexOf(member);
        this.onl_usr_panel.insertBefore(this.create_name(member, true), this.onl_usr_panel.children[index]);
    }
    usr_switch_off(member) {
        if (this.signingout) {
            this.signingout = false;
            return;
        }
        let index = this.arr_onl_usrs.indexOf(member);
        this.arr_onl_usrs.splice(index, 1);
        this.remove_el(this.onl_usr_panel.children[index]);
        this.arr_ofl_usrs.push(member);
        this.arr_ofl_usrs.sort(function (a, b) {
            if (a.toLowerCase() < b.toLowerCase()) return -1;
            if (a.toLowerCase() > b.toLowerCase()) return 1;
            return 0;
        });
        index = this.arr_ofl_usrs.indexOf(member);
        this.ofl_usr_panel.insertBefore(this.create_name(member, false), this.ofl_usr_panel.children[index]);
    }
    member_signout(member) {
        let index = this.arr_onl_usrs.indexOf(member);
        if (index > -1) {
            this.arr_onl_usrs.splice(index, 1);
            this.remove_el(this.onl_usr_panel.children[index]);
        }
        this.signingout = true;
    }
    remove_el(el) {
        if (this.onl_usrs.has(el)) {
            if (this.onl_usrs.size == 1) {
                if (this.is_cleared == true) {
                    this.is_cleared = false;
                    this.btn_swtch.firstElementChild.src = "/images/cancel_sel.svg";
                }
                this.btn_swtch.classList.add("disabled");
                this.onl_usrs.clear();
            }
            else
                this.onl_usrs.delete(el);
        }
        el.remove();
    }
    jsMsToTicks(jsMs) {
        return 621355968000000000 + jsMs * 10000;
    }
    ticksToJsMs(ticks) {
        return (ticks - 621355968000000000) / 10000;
    }
    form_time(jsMs) {
        let time;
        let month;
        let day;
        let hours;
        let minutes;
        if (jsMs != 0) {
            time = new Date(jsMs);
            month = time.getMonth().toString();
            if (month.length < 2)
                month = '0' + month;
            day = time.getDay().toString();
            if (day.length < 2)
                day = '0' + day;
            hours = time.getHours().toString();
            if (hours.length < 2)
                hours = '0' + hours;
            minutes = time.getMinutes().toString();
            if (minutes.length < 2)
                minutes = '0' + minutes;
        }
        else
            month = day = hours = minutes = "--";
        return month + '-' + day + '/' + hours + ':' + minutes;
    }
    form_msg(msg) {
        let div = document.createElement("div");
        div.classList.add("message");
        div.appendChild(document.createElement("div"));
        div.appendChild(document.createElement("div"));
        div.firstElementChild.appendChild(document.createElement("img"));
        div.firstElementChild.appendChild(document.createElement("div"));
        div.firstElementChild.appendChild(document.createElement("div"));
        div.firstElementChild.appendChild(document.createElement("div"));
        div.firstElementChild.children[1].textContent = this.form_time(msg.jsTime);
        if (msg.peers) {
            div.firstElementChild.firstElementChild.src = "/images/reply.svg";
            div.firstElementChild.firstElementChild.setAttribute("draggable", "false");
            div.firstElementChild.firstElementChild.setAttribute("onclick", "app.groupin.reply_click(this)");
            div.firstElementChild.firstElementChild.classList.add("reply");
            div.firstElementChild.children[2].textContent = "Secret";
            div.firstElementChild.children[2].classList.add("secret");
            div.firstElementChild.children[2].setAttribute("onclick", "app.groupin.show_peers(this)");
            let ul = document.createElement("ul");
            msg.peers.forEach(peer => {
                let li = document.createElement("li");
                li.textContent = peer;
                ul.appendChild(li);
            });
            div.appendChild(ul);
        }
        else {
            div.firstElementChild.firstElementChild.src = "/images/message.svg";
            div.firstElementChild.firstElementChild.setAttribute("draggable", "false");
            div.firstElementChild.children[2].textContent = "Public";
        }
        div.firstElementChild.children[3].textContent = msg.from;
        div.children[1].textContent = msg.text;
        return div;
    }
    recieve_msg(msg) {
        this.last_msg_time = msg.stringTime;
        this.msgs_cont.appendChild(this.form_msg(msg));
        if (this.isdown)
            this.msgs_cont.scrollTop = this.msgs_cont.scrollHeight;
    }
    onkey_input(ev, el) {
        if (ev.keyCode == 13 && el.value) {
            var msg = null;
            if (this.onl_usrs.size == 0 || this.is_cleared == true) {
                msg = {
                    to: null,
                    text: el.value
                };
            }
            else {
                let arr = [];
                this.onl_usrs.forEach(user => arr.push(user.textContent));
                msg = {
                    to: arr,
                    text: el.value
                };
            }
            el.value = null;
            let msgLoc = {
                time: 0,
                from: app.name,
                peers: msg.to,
                text: msg.text
            };
            this.last_sent_message = this.form_msg(msgLoc);
            this.msgs_cont.appendChild(this.last_sent_message);
            this.msgs_cont.scrollTop = this.msgs_cont.scrollHeight;
            this.connection.invoke("MessageServer", msg).then(time => {
                this.last_msg_time = time.stringTime;
                this.last_sent_message.firstElementChild.children[1].textContent = this.form_time(time.jsTime);
            }).catch(err => {
                this.last_sent_message.children[1].style.color = "red";
                let msg = err.message;
                let index = msg.indexOf("HubException:");
                if (index > -1)
                    msg = msg.substring(index + 14);
                app.alert(msg);
            });
        }
    }
    show_peers(el) {
        let msg_shell = el.parentElement.parentElement;
        if (this.open_peers != null)
            this.open_peers.classList.remove("showpeers");
        if (this.open_peers != msg_shell) {
            this.open_peers = msg_shell;
            this.open_peers.classList.add("showpeers");
        }
        else
            this.open_peers = null;
    }
    reply_click(el) {
        let msg_shell = el.parentElement.parentElement;
        let list = [];
        for (let i = 0; i < msg_shell.children[2].children.length; i++) {
            if (msg_shell.children[2].children[i].textContent != app.name)
                list.push(msg_shell.children[2].children[i].textContent);
        }
        if (msg_shell.firstElementChild.children[3].textContent != app.name)
            list.push(msg_shell.firstElementChild.children[3].textContent);
        let cleared = false;
        if (list.length > 0) {
            for (let i = 0; i < this.onl_usr_panel.children.length; i++) {
                let index = list.indexOf(this.onl_usr_panel.children[i].textContent);
                if (index > -1) {
                    if (!cleared) {
                        cleared = true;
                        if (this.onl_usrs.size > 0) {
                            if (this.is_cleared == true) {
                                this.is_cleared = false;
                                this.btn_swtch.firstElementChild.src = "/images/cancel_sel.svg";
                            }
                            this.onl_usrs.clear();
                        }
                        else {
                            this.btn_swtch.classList.remove("disabled");
                        }
                        for (let i = 0; i < this.onl_usr_panel.children.length; i++)
                            this.onl_usr_panel.children[i].classList.remove("selected");
                    }
                    this.onl_usr_panel.children[i].classList.add("selected");
                    this.onl_usrs.add(this.onl_usr_panel.children[i]);
                    list.splice(index, 1);
                }
            }
        }
        if (cleared)
            this.msgs_panel.children[2].focus();
    }
    get_msgs(scroll = false) {
        if (!this.end_reached) {
            app.api.get_grp_msgs(this.upper_msg_time, this.msgs_quantity).then(ret => {
                if (ret) {
                    if (isNaN(ret)) {
                        if (Array.isArray(ret)) {
                            if (ret.length < this.msgs_quantity)
                                this.end_reached = true;
                            let initiated = false;
                            if (!this.end_reached)
                                this.upper_msg_time = ret[0].stringTime;
                            if (!this.upper_msg_el)
                                this.last_msg_time = ret[ret.length - 1].stringTime;
                            let Previous = this.upper_msg_el;
                            ret.forEach(msg => {
                                let div = this.form_msg(msg);
                                if (!initiated && !this.end_reached) {
                                    this.upper_msg_el = div;
                                    initiated = true;
                                }
                                if (Previous)
                                    this.msgs_cont.insertBefore(div, Previous);
                                else this.msgs_cont.appendChild(div);
                            });
                            if (scroll)
                                clearTimeout(this.scroll_handle);
                                this.scroll_handle = setTimeout(() => {
                                    app.groupin.msgs_cont.scrollTop = app.groupin.msgs_cont.scrollHeight;
                                    app.groupin.msgs_cont.style.opacity = "1";
                                    app.groupin.msgs_cont.style.scrollBehavior = "smooth";
                                }, 150);
                            if (this.msgs_cont.scrollHeight < this.msgs_cont.offsetHeight * 3)
                                this.get_msgs(scroll);
                        }
                        else {
                            app.alert("Error. Server returned: " + ret);
                        }
                    }
                    else {
                        if (ret == 0)
                            this.end_reached = true;
                        else
                            app.alert("Something went wrong, try to reload the page");
                    }
                }
            });
        }
    }
    get_missed_msgs() {
        app.api.get_missed_msgs(this.last_msg_time).then(ret => {
            if (ret) {
                if (isNaN(ret)) {
                    if (Array.isArray(ret)) {
                        ret.forEach(msg => this.msgs_cont.appendChild(this.form_msg(msg)));
                        this.last_msg_time = ret[ret.length - 1].stringTime;
                        if (this.isdown)
                            this.msgs_cont.scrollTop = this.msgs_cont.scrollHeight;
                    }
                }
                else {
                    if (ret == -1)
                        app.alert("Limit exceeded. Try to reload the page.");
                }
            }
        });
    }
    scroll() {
        //isdown when down
    }
    //todo implement loading messages from server
    //todo implement scrolling on messages
    //edge doesn't show arrow pointer on some elements
    //implement encryption word or sentence (maybe only word by trimming spaces) testing
}

class app_class {
    constructor() {
        this.page = null;
        this.name = null;
        this.group = null;
        this.ingroup = null;
        this.api = new api_class();
        this.reg_panel = new reg_panel_class();
        this.groups = new groups_class();
        this.groupin = new ingroup_class();
        setTimeout(() => {
            this.api.usr_info().then(ret => {
                if (ret) {
                    if (this.ingroup)
                        this.goto('ingroup');
                    else
                        this.goto('groups');
                }
            });
        }, 1300);
        this.message = {
            password: text => text + " must be at least 8 characters long and maximum 32",
            name: text => text + " must be at least 5 characters long and maximum 64"
        };
    }
    on_resize() {
        switch (this.page) {
            case 'groups':
                this.groups.groups_resize();
                break;
            case 'ingroup':
                this.groupin.ingroup_resize();
                break;
        }
    }
    wait() {
        document.getElementById('wait').style.visibility = 'visible';
    }
    resume() {
        document.getElementById('wait').style.visibility = 'hidden';
    }
    alert(message) {
        let el = document.getElementById('message');
        el.children[0].children[1].focus();
        el.children[0].children[0].textContent = message;
        el.style.display = 'block';
    }
    goto(place) {
        if (this.page == place)
            return;
        switch (place) {
            case 'reg':
                document.body.children[0].style.display = 'block';
                this.hide('reg');
                break;
            case 'groups':
                this.groups.list_clear();
                this.groups.list_load();
                this.groups.groups_window.getElementsByTagName('code')[0].textContent = this.name;
                this.groups.groups_window.getElementsByTagName('code')[0].title = this.name;
                document.body.children[1].style.display = 'block';
                this.groups.initialize();
                this.hide('groups');
                break;
            case 'ingroup':
                this.groupin.connection.start().then(() => {
                    this.groupin.init();
                    this.groupin.open_btn.nextElementSibling.textContent = this.ingroup;
                    this.groupin.open_btn.nextElementSibling.title = 'In Group: ' + this.ingroup;
                    document.body.children[2].style.display = 'block';
                    document.addEventListener("visibilitychange", this.visib_change);
                    this.hide('ingroup');
                });
                break;
        }
    }
    hide(place) {
        switch (this.page) {
            case 'reg':
                document.body.children[0].style.display = 'none';
                try {
                    app.reg_panel.goto('home');
                }
                catch (err) { }
                break;
            case 'groups':
                document.body.children[1].style.display = 'none';
                this.groups.groups_window.children[2].children[1].value = null;
                this.groups.query = '';
                break;
            case 'ingroup':
                this.groupin.connection.stop().then(() => {
                    document.body.children[2].style.display = 'none';
                    document.removeEventListener("visibilitychange", this.visib_change);
                });
                break;
        }
        this.page = place;
    }
    validatePassword(password) {
        if (!password || password.length < 8 || password.length > 32)
            return false;
        else
            return true;
    }
    validateName(name) {
        if (!name || name.length < 5 || name.length > 64)
            return false;
        else
            return true;
    }
    visib_change() {
        switch (document.visibilityState) {
            case "hidden":
                app.groupin.connection.stop().catch(err => alert(err));
                break;
            case "visible":
                app.groupin.connection.start().then(() => app.groupin.init()).catch(err => app.alert(err));
                break;
        }
    }
}