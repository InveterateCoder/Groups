var app;
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
            if (resp.startsWith('OK_')) {
                let index = resp.indexOf('_', 3);
                let len = Number(resp.substring(3, index));
                localStorage.setItem('name', resp.substring(index + 1, index + 1 + len));
                let group = resp.substring(index + 1 + len);
                if (group)
                    localStorage.setItem('group', group);
                else
                    localStorage.removeItem('group');
                ret = true;
            }
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
            if (resp == 'OK')
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
                    localStorage.setItem('name', request.NewName);
                    break;
                case "pass_changed":
                    ret.pass = true;
                    break;
                case "name&pass_changed":
                    ret.name = true;
                    ret.pass = true;
                    localStorage.setItem('name', request.NewName);
                    break;
                case "wrong_password":
                    app.alert("Wrong password")
                    break;
                case "no_change_requested":
                    app.alert("Empty request");
                    break;
                case "same_credentials":
                    app.alert("No changes have been made, same credentials");
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
                    localStorage.setItem('group', info.GroupName);
                    ret = true;
                    break;
                case "has_group":
                    app.alert("You have already registered a group");
                    break;
                case "name_taken":
                    app.alert("A group with such name already exists");
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
                    localStorage.setItem('group', info.NewGroupName);
                case "pass_changed":
                    ret = true;
                    break;
                case "has_no_group":
                    localStorage.removeItem('group');
                    ret = true;
                    break;
                case "wrong_password":
                    app.alert("Wrong password");
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
            ret = await resp.text();
            if (ret == 'not_authorized' || ret == 'single_connection_only')
                location.reload();
        }
        catch (err) {
            ret = err.message;
        }
        app.resume();
        return ret;
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
        if (!app.nameValid(chatterer.name))
            app.alert("The name's length should be minimum 5 characters and maximum 64");
        else if (!chatterer.email || !form[1].checkValidity())
            app.alert('Incorrect email address');
        else if (!app.passwordValid(chatterer.password))
            app.alert("The password's length should be minimum 8 characters and maximum 32");
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
        else if (!app.passwordValid(chatterer.password))
            app.alert("The password's length should be minimum 8 characters and maximum 32");
        else app.api.signin(chatterer).then(ok => {
            if (ok)
                app.goto('groups');
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
        this.account_btn = this.groups_window.children[1].children[2].children[0].children[0];
        this.group_btn = this.groups_window.children[1].children[2].children[0].children[1];
        this.acc_open = false;
        this.group_open = false;
        this.groups_forms = document.getElementById('groups_forms');
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
        let height = this.groups_window.children[1].offsetHeight;
        this.m_acc.style.right = '0px';
        this.m_acc.style.top = height + 'px';
        this.m_acc.style.width = this.account_btn.offsetWidth + 28 + 'px';
        this.m_group.style.right = this.m_acc.offsetWidth + 'px';
        this.m_group.style.top = height + 'px';
        this.m_group.style.width = this.group_btn.offsetWidth + 28 + 'px';
        if (group)
            this.group_conf();
        this.m_all.style.top = this.groups_window.children[1].offsetHeight + 'px';
        this.m_all.style.transform = `translate(0, -${this.m_all.offsetHeight}px)`;
    }
    group_conf() {
        if (localStorage.getItem('group')) {
            this.m_group.children[0].style.display = 'none';
            this.m_group.children[1].textContent = localStorage.getItem('group');
            this.m_group.children[1].style.display = 'list-item';
            this.m_group.children[2].style.display = 'list-item';
            this.m_group.children[3].style.display = 'list-item';
            this.m_all.children[5].style.display = 'none';
            this.m_all.children[6].textContent = localStorage.getItem('group')
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
        this.close_all_menus();
        this.initialize(false);
    }
    signout() {
        this.close_all_menus();
        app.api.signout().then(ok => {
            if (ok) {
                app.goto('reg');
                localStorage.removeItem('name');
                localStorage.removeItem('group');
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
        if (!app.passwordValid(request.Password))
            app.alert("Password must be at least 8 characters long and maximum 32");
        else if (request.NewName.length > 0 && !app.nameValid(request.NewName))
            app.alert("New name must be at least 5 characters long and maximum 64");
        else if (request.NewPassword.length > 0 && !app.passwordValid(request.NewPassword))
            app.alert("New password must be at least 8 characters long and maximum 32");
        else if (request.Password == request.NewPassword || request.NewName == localStorage.getItem('name'))
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
                        this.groups_window.getElementsByTagName('code')[0].textContent = localStorage.getItem('name');
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
        else if (!app.passwordValid(signin.password))
            app.alert("Password must be at least 8 characters long and maximum 32");
        else {
            app.api.acc_delete(signin).then(ret => {
                if (ret) {
                    app.goto('reg');
                    localStorage.removeItem('name');
                    localStorage.removeItem('group');
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
        if (!app.passwordValid(info.Password))
            app.alert("Password must be at least 8 characters long and maximum 32");
        else if (!app.nameValid(info.GroupName))
            app.alert("Group name must be at least 5 characters long and maximum 64");
        else if (info.GroupPassword.length > 0 && !app.passwordValid(info.GroupPassword))
            app.alert("Group password must be at least 8 characters long and maximum 32");
        else {
            if (info.GroupPassword.length == 0)
                info.GroupPassword = null;
            app.api.grp_reg(info).then(ret => {
                if (ret) {
                    this.group_conf();
                    this.form_close(3);
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
        if (!app.passwordValid(info.Password))
            app.alert("Password must be at least 8 characters long and maximum 32");
        else if (info.NewGroupName.length > 0 && !app.nameValid(info.NewGroupName))
            app.alert("New name must be at least 5 characters long and maximum 64");
        else if (!form[2].checked && info.NewGroupPassword.length > 0 && !app.passwordValid(info.NewGroupPassword))
            app.alert("New password must be at least 8 characters long and maximum 32");
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
                        this.form_close(4);
                    }
                });
            }
        }

    }
    on_pass_del_change(el) {
        if (el.checked)
            this.groups_forms.children[4].children[4].style.visibility = 'hidden';
        else
            this.groups_forms.children[4].children[4].style.visibility = 'visible';
    }
}

class app_class {
    constructor() {
        this.api = new api_class();
        this.reg_panel = new reg_panel_class();
        this.groups = new groups_class();
        if (localStorage.getItem('page') == null || !document.cookie.startsWith('Chat_Authentication_Token')) {
            if (document.cookie.startsWith('Chat_Authentication_Token'))
                document.cookie = 'Chat_Authentication_Token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
            if (localStorage.getItem('name'))
                localStorage.removeItem('name');
            if (localStorage.getItem('group'))
                localStorage.removeItem('group');
            this.goto('reg');
        }
        else
            this.goto(localStorage.getItem('page'));
    }
    on_resize() {
        this.groups.groups_resize();
    }
    wait() {
        document.getElementById('wait').style.visibility = 'visible';
    }
    resume() {
        document.getElementById('wait').style.visibility = 'hidden';
    }
    alert(message) {
        let el = document.getElementById('message');
        el.children[0].children[0].textContent = message;
        el.style.display = 'block';
    }
    goto(place) {
        switch (place) {
            case 'reg':
                if (localStorage.getItem('page') != 'reg')
                    this.hide();
                document.body.style.backgroundColor = 'rgb(0, 3, 56)';
                document.body.style.backgroundImage = 'url("/images/concrete-wall-2.png")';
                document.body.children[0].style.display = 'block';
                localStorage.setItem('page', place);
                break;
            case 'groups':
                if (localStorage.getItem('page') != 'groups')
                    this.hide();
                document.body.style.backgroundColor = '#efefef';
                document.body.style.backgroundImage = 'url("/images/low-contrast-linen.png")';
                this.groups.groups_window.getElementsByTagName('code')[0].textContent = localStorage.getItem('name');
                document.body.children[1].style.display = 'block';
                localStorage.setItem('page', place);
                this.groups.initialize();
                break;
            case 'ingroup':
                if (localStorage.getItem('page') != 'ingroup')
                    this.hide();
                document.body.children[2].style.display = 'block';
                localStorage.setItem('page', place);
                break;
        }
    }
    hide() {
        switch (localStorage.getItem('page')) {
            case 'reg':
                document.body.children[0].style.display = 'none';
                try {
                    app.reg_panel.goto('home');
                }
                catch(err){}
                break;
            case 'groups':
                document.body.children[1].style.display = 'none';
                break;
            case 'ingroup':
                document.body.children[2].style.display = 'none';
                break;
        }
    }
    passwordValid(password) {
        if (!password || password.length < 8 || password.length > 32)
            return false;
        else
            return true;
    }
    nameValid(name) {
        if (!name || name.length < 5 || name.length > 64)
            return false;
        else
            return true;
    }
}